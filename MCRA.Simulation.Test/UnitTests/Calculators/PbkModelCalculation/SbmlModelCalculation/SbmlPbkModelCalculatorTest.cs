using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation.SbmlModelCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.PbkModelCalculation.SbmlModelCalculation {

    /// <summary>
    /// SBML KineticModelCalculation calculator tests.
    /// </summary>
    [TestClass]
    public class SbmlPbkModelCalculatorTest : PbkModelCalculatorTestsBase {

        protected virtual KineticModelDefinition GetModelDefinition() {
            return KineticModelDefinition.FromSbmlFile(
                "Resources/PbkModels/EuroMixGenericPbk.sbml"
            );
        }

        protected override KineticModelInstance getDefaultInstance(params Compound[] substance) {
            var instance = createFakeModelInstance(substance.Single());
            return instance;
        }

        protected override PbkSimulationSettings getDefaultSimulationSettings() {
            return new PbkSimulationSettings() {
                NumberOfSimulatedDays = 10,
                UseRepeatedDailyEvents = true,
            };
        }

        protected override PbkModelCalculatorBase createCalculator(
            KineticModelInstance instance,
            PbkSimulationSettings simulationSettings
        ) {
            return new SbmlPbkModelCalculator(instance, simulationSettings);
        }

        protected override TargetUnit getDefaultInternalTarget() {
            return TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
        }

        protected override TargetUnit getDefaultExternalTarget() {
            return TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
        }

        [TestMethod]
        public void SbmlPbkModelCalculator_TestDefinition() {
            var modelDefinition = GetModelDefinition();
            var expected = new[] { "QGut", "QSkin_sc_e", "QAir" };
            CollectionAssert.AreEquivalent(
                expected,
                modelDefinition.Forcings.Select(r => r.Id).ToArray()
            );
            var bwParam = modelDefinition.GetParameterDefinitionByType(PbkModelParameterType.BodyWeight);
            Assert.AreEqual("BM", bwParam.Id);
            var bsaParam = modelDefinition.GetParameterDefinitionByType(PbkModelParameterType.BodySurfaceArea);
            Assert.AreEqual("BSA", bsaParam.Id);
        }

        [TestMethod]
        [DataRow(ExposureRoute.Oral)]
        [DataRow(ExposureRoute.Dermal)]
        [DataRow(ExposureRoute.Inhalation)]
        public override void TestForwardAcute(ExposureRoute route) {
            testForwardAcute(route);
        }

        [TestMethod]
        [DataRow(ExposureRoute.Oral)]
        [DataRow(ExposureRoute.Dermal)]
        [DataRow(ExposureRoute.Inhalation)]
        public override void TestForwardChronic(ExposureRoute route) {
            testForwardChronic(route);
        }

        /// <summary>
        /// SBML ModelCalculator: calculates individual day target exposures, Euromix SBML, acute
        /// </summary>
        [TestMethod]
        public void SbmlModelCalculator_TestCalculateIndividualDayTargetExposuresEuromix() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var routes = new[] { ExposureRoute.Oral, ExposureRoute.Dermal, ExposureRoute.Inhalation };
            var paths = FakeExposurePathGenerator.Create(routes);
            var individuals = FakeIndividualsGenerator.Create(5, 2, random, useSamplingWeights: true);
            var individualDays = FakeIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = FakeExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, paths, seed);
            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);

            var instance = createFakeModelInstance(substance);
            var simulationSettings = new PbkSimulationSettings() {
                NumberOfSimulatedDays = 10,
                NumberOfOralDosesPerDay = 1,
                SpecifyEvents = true,
                SelectedEvents = [1, 2, 4, 6, 8, 9, 10]
            };
            var model = new SbmlPbkModelCalculator(instance, simulationSettings);

            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            var forwardCalculator = new PbkKineticConversionCalculator(model);
            var internalExposures = forwardCalculator.CalculateIndividualDayTargetExposures(
                individualDayExposures,
                routes,
                ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay),
                [targetUnit],
                new ProgressState(),
                random
            );
            sw.Stop();
            System.Diagnostics.Trace.WriteLine(message: $"Elapsed: {sw.Elapsed}");
            var positiveExternalExposures = individualDayExposures
                .Where(r => r.ExposuresPerPath
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
            Assert.AreEqual(10 * 24 + 1, targetExposurePattern.TargetExposuresPerTimeUnit.Count);
        }

        private KineticModelInstance createFakeModelInstance(
            Compound substance
        ) {
            var modelDefinition = GetModelDefinition();
            var idModelInstance = modelDefinition.IdModel;
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
                    DistributionType = PbkModelParameterDistributionType.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVRich",
                    Value = 0.105,
                    DistributionType = PbkModelParameterDistributionType.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVLiver",
                    Value = 0.024,
                    DistributionType = PbkModelParameterDistributionType.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVBlood",
                    Value = 0.068,
                    DistributionType = PbkModelParameterDistributionType.LogisticNormal,
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
                    DistributionType = PbkModelParameterDistributionType.LogNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFFat",
                    Value = 0.046,
                    DistributionType = PbkModelParameterDistributionType.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFPoor",
                    Value = 0.134,
                    DistributionType = PbkModelParameterDistributionType.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFLiver",
                    Value = 0.259,
                    DistributionType = PbkModelParameterDistributionType.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFSkin",
                    Value = 0.054,
                    DistributionType = PbkModelParameterDistributionType.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Falv",
                    Value = 2220,
                    DistributionType = PbkModelParameterDistributionType.LogNormal,
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
                    DistributionType = PbkModelParameterDistributionType.LogNormal,
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
                    DistributionType = PbkModelParameterDistributionType.LogNormal,
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
                    DistributionType = PbkModelParameterDistributionType.LogisticNormal,
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

            var instance = new KineticModelInstance() {
                KineticModelDefinition = modelDefinition,
                KineticModelSubstances = [
                     new KineticModelSubstance() {
                         Substance = substance
                     }
                ],
                KineticModelInstanceParameters = kineticModelParameters
                    .ToDictionary(r => r.Parameter),
                IdTestSystem = "Human",
                IdModelDefinition = modelDefinition.Id,
            };
            return instance;
        }
    }
}
