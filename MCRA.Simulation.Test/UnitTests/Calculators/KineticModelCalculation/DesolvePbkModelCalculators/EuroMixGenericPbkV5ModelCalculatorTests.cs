using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Logger;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.R.REngines;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation.DesolvePbkModelCalculators {

    /// <summary>
    /// KineticModelCalculation calculator
    /// </summary>
    [TestClass]
    public class EuroMixGenericPbkV5ModelCalculatorTests : PbkModelCalculatorBaseTests {

        protected override KineticModelInstance getDefaultInstance(params Compound[] substance) {
            var instance = createFakeModelInstance(substance.Single());
            instance.NumberOfDays = 10;
            instance.NumberOfDosesPerDay = 1;
            instance.NonStationaryPeriod = 5;
            return instance;
        }

        protected override PbkModelCalculatorBase createCalculator(KineticModelInstance instance) {
            return new CosmosKineticModelCalculator(instance);
        }

        protected override TargetUnit getDefaultInternalTarget() {
            return TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
        }

        protected override TargetUnit getDefaultExternalTarget() {
            return TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
        }

        [TestMethod]
        [DataRow(ExposureRoute.Oral)]
        [DataRow(ExposureRoute.Dermal)]
        [DataRow(ExposureRoute.Inhalation)]
        public override void TestForwardAcute(ExposureRoute exposureRoute) {
            testForwardAcute(exposureRoute);
        }

        [TestMethod]
        [DataRow(ExposureRoute.Oral)]
        [DataRow(ExposureRoute.Dermal)]
        [DataRow(ExposureRoute.Inhalation)]
        public override void TestForwardChronic(ExposureRoute exposureRoute) {
            testForwardChronic(exposureRoute);
        }

        /// <summary>
        /// CosmosModelCalculator: calculates individual day target exposures, CosmosV4, acute
        /// </summary>
        [TestMethod]
        public void EuroMixGenericPbkV5ModelCalculator_TestAcute() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new List<ExposurePathType>() { ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = MockExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, routes, seed);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);

            var instance = createFakeModelInstance(substance);
            instance.NumberOfDays = 100;
            instance.NumberOfDosesPerDay = 1;

            var calculator = new CosmosKineticModelCalculator(instance);
            var internalExposures = calculator
                .CalculateIndividualDayTargetExposures(
                    individualDayExposures,
                    routes,
                    ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay),
                    new List<TargetUnit> { targetUnit },
                    new ProgressState(),
                    random
                );

            Assert.AreEqual(50, internalExposures.Count);

            var positiveExternalExposures = individualDayExposures
                .Where(r => r.ExposuresPerRouteSubstance
                .Any(eprc => eprc.Value.Any(ipc => ipc.Amount > 0)))
                .ToList();
            var positiveInternalExposures = internalExposures
                .Where(r => r.IsPositiveTargetExposure(targetUnit.Target))
                .ToList();
            Assert.AreEqual(
                positiveExternalExposures.Count,
                positiveInternalExposures.Count
            );

            var targetExposurePattern = positiveInternalExposures.First()
                .GetSubstanceTargetExposure(targetUnit.Target, substance) as SubstanceTargetExposurePattern;
            Assert.AreEqual(100 * 24 + 1, targetExposurePattern.TargetExposuresPerTimeUnit.Count);
        }

        /// <summary>
        /// CosmosModelCalculator: calculates individual  target exposures, CosmosV4, chronic
        /// </summary>
        [TestMethod]
        public void EuroMixGenericPbkV5ModelCalculator_TestChronic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new List<ExposurePathType>() { ExposurePathType.Oral, ExposurePathType.Dermal, ExposurePathType.Inhalation };
            var individuals = MockIndividualsGenerator.Create(25, 2, random, useSamplingWeights: true);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = MockExternalExposureGenerator.CreateExternalIndividualExposures(individualDays, substances, routes, seed);

            var instance = createFakeModelInstance(substance);
            instance.NumberOfDays = 100;
            instance.NumberOfDosesPerDay = 1;
            instance.NonStationaryPeriod = 10;
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);

            var calculator = new CosmosKineticModelCalculator(instance);
            var outputPath = CreateTestOutputPath("TestChronic");
            using (var logger = new FileLogger(Path.Combine(outputPath, "AnalysisCode.R"))) {
                calculator.CreateREngine = () => new LoggingRDotNetEngine(logger);

                var internalExposures = calculator.CalculateIndividualTargetExposures(
                    individualExposures,
                    routes,
                    ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay),
                    new List<TargetUnit> { targetUnit },
                    new ProgressState(),
                    random
                );

                Assert.AreEqual(25, internalExposures.Count);

                var positiveExternalExposures = individualExposures
                    .Where(r => r.ExposuresPerRouteSubstance
                    .Any(eprc => eprc.Value.Any(ipc => ipc.Amount > 0)))
                    .ToList();
                var positiveInternalExposures = internalExposures
                    .Where(r => r.IsPositiveTargetExposure(targetUnit.Target))
                    .ToList();
                Assert.AreEqual(
                    positiveExternalExposures.Count,
                    positiveInternalExposures.Count
                );

                var targetExposurePattern = positiveInternalExposures.First()
                    .GetSubstanceTargetExposure(targetUnit.Target, substance) as SubstanceTargetExposurePattern;
                Assert.AreEqual(100 * 24 + 1, targetExposurePattern.TargetExposuresPerTimeUnit.Count);
            }
        }

        /// <summary>
        /// Creates a COSMOS v5 kinetic model instance.
        /// </summary>
        /// <param name="substance"></param>
        /// <returns></returns>
        private static KineticModelInstance createFakeModelInstance(Compound substance) {
            var idModelDefinition = "EuroMix_Generic_PBTK_model_V5";
            var idModelInstance = $"{idModelDefinition}-{substance.Code}";
            var kineticModelParameters = new List<KineticModelInstanceParameter> {
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "BM",
                    Value = 70,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "BSA",
                    Value = 190,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVFat",
                    Value = 0.209,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVRich",
                    Value = 0.105,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVLiver",
                    Value = 0.024,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVBlood",
                    Value = 0.068,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Height_sc",
                    Value = 0.0001,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Height_vs",
                    Value = 0.0122,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFBlood",
                    Value = 4.8,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFFat",
                    Value = 0.046,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFPoor",
                    Value = 0.134,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFLiver",
                    Value = 0.259,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFSkin",
                    Value = 0.054,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Falv",
                    Value = 2220,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "mic",
                    Value = 52.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCAir",
                    Value = 1e99,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCFat",
                    Value = 31.11,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCPoor",
                    Value = 3.03,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCRich",
                    Value = 1.92,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCLiver",
                    Value = 4.95,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCSkin",
                    Value = 3.71,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCSkin_sc",
                    Value = 0.1,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Kp_sc_vs",
                    Value = 0.1,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Ke",
                    Value = 7.5,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Michaelis",
                    Value = 1,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Vmax",
                    Value = 0.26,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Km",
                    Value = 0.00484,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "CLH",
                    Value = 0,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "fup",
                    Value = 0.11,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Frac",
                    Value = 1,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.3,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "kGut",
                    Value = 1.3,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "fSA_exposed",
                    Value = 1,
                }
            };
            var kineticModel = new KineticModelInstance() {
                IdModelInstance = idModelInstance,
                KineticModelInstanceParameters = kineticModelParameters.ToDictionary(r => r.Parameter),
                KineticModelDefinition = MCRAKineticModelDefinitions.Definitions[idModelDefinition],
                KineticModelSubstances = new List<KineticModelSubstance>() {
                    new KineticModelSubstance() {
                        Substance = substance
                    }
                },
                IdModelDefinition = idModelDefinition,
                IdTestSystem = "Human",
            };
            return kineticModel;
        }
    }
}
