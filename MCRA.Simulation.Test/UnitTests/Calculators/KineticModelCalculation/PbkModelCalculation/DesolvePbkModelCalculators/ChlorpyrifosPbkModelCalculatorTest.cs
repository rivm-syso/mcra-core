using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.DesolvePbkModelCalculators.ChlorpyrifosPbkModelCalculation;
using MCRA.Simulation.Calculators.TargetExposuresCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.ProgressReporting;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation.PbkModelCalculation.DesolvePbkModelCalculators {

    /// <summary>
    /// KineticModelCalculation calculator
    /// </summary>
    [TestClass]
    public class ChlorpyrifosPbkModelCalculatorTest : PbkModelCalculatorBaseTests {

        protected override KineticModelInstance getDefaultInstance(
            params Compound[] substances
        ) {
            var instance = createFakeModelInstance(
                "FakeCPFInstance",
                substances.ToList()
            );
            instance.NumberOfDays = 10;
            instance.NumberOfDosesPerDay = 1;
            instance.NonStationaryPeriod = 5;
            return instance;
        }

        protected override PbkModelCalculatorBase createCalculator(KineticModelInstance instance) {
            return new ChlorpyrifosPbkModelCalculator(instance);
        }

        protected override TargetUnit getDefaultInternalTarget() {
            return TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.BloodPlasma);
        }

        protected override TargetUnit getDefaultExternalTarget() {
            return TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);
        }

        [TestMethod]
        [DataRow(ExposureRoute.Oral)]
        public override void TestForwardAcute(ExposureRoute exposureRoute) {
            testForwardAcute(exposureRoute);
        }

        [TestMethod]
        [DataRow(ExposureRoute.Oral)]
        public override void TestForwardChronic(ExposureRoute exposureRoute) {
            testForwardChronic(exposureRoute);
        }

        /// <summary>
        /// ChlorpyrifosPbkModelCalculator: calculates individual day target exposures, V1, acute
        /// </summary>
        [TestMethod]
        [DataRow(BiologicalMatrix.VenousBlood)]
        [DataRow(BiologicalMatrix.Excreta)]
        [DataRow(BiologicalMatrix.BloodPlasma)]
        public void ChlorpyrifosPbkModelCalculator_TestCalculateIndividualDayTargetExposures(
            BiologicalMatrix biologicalMatrix
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = MockSubstancesGenerator.Create(3);
            var routes = new[] { ExposurePathType.Oral };
            var individuals = FakeIndividualsGenerator.Create(2, 2, random);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualDayExposures = MockExternalExposureGenerator.CreateExternalIndividualDayExposures(individualDays, substances, routes, seed);

            var instance = getDefaultInstance(substances.ToArray());
            instance.NumberOfDays = 10;
            instance.NumberOfDosesPerDay = 1;
            instance.NonStationaryPeriod = 0;

            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.mgPerL, biologicalMatrix);
            var model = new ChlorpyrifosPbkModelCalculator(instance);
            var internalExposures = model.CalculateIndividualDayTargetExposures(
                individualDayExposures,
                routes,
                getDefaultExternalTarget().ExposureUnit,
                [targetUnit],
                new ProgressState(),
                random
            );

            Assert.AreEqual(4, internalExposures.Count);
        }

        /// <summary>
        /// ChlorpyrifosKineticModelCalculator: ; simulate as in Berkely Madonna 
        /// GDOSE = 0.5                        	; Dose in mg/kg bw(given dose)
        /// ODOSE = GDOSE* 1e-3 / MWP*1e6   	; Dose in umol/kg bw(oral dose)
        /// DOSE = ODOSE* BW; Dose in umol
        /// BW = 70
        /// MWP = 350.59
        /// </summary>
        [TestMethod]
        [DataRow(BiologicalMatrix.VenousBlood, 0.0005, "P")]
        [DataRow(BiologicalMatrix.Excreta, 0.3975, "M2")]
        public void ChlorpyrifosPbkModelCalculator_TestCalculateIndividualTargetExposures(
            BiologicalMatrix biologicalMatrix,
            double expectedSteadyState,
            string codeFocalSubstance
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substanceCodes = new[] { "P", "M1", "M2" };
            var substances = substanceCodes
                .Select(r => new Compound(r) {
                    MolecularMass = r == "P" ? 350.59 : 291
                })
                .ToList();
            var metabolites = substances.Skip(1).ToList();
            var routes = new List<ExposurePathType>() { ExposurePathType.Oral };
            var individualExposures = createFakeExternalIndividualExposures(
                seed,
                substances,
                .5,
                routes
            );

            var instance = getDefaultInstance(substances.ToArray());
            instance.NumberOfDays = 10;
            instance.NumberOfDosesPerDay = 1;
            instance.NonStationaryPeriod = 5;

            var targetUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.mgPerL, biologicalMatrix);
            var model = new ChlorpyrifosPbkModelCalculator(instance);
            var internalExposures = model.CalculateIndividualTargetExposures(
                individualExposures,
                routes,
                ExposureUnitTriple.FromDoseUnit(DoseUnit.mgPerKg),
                [targetUnit],
                new ProgressState(),
                random
            );
            var substanceTargetExposurePattern = internalExposures
                .Select(r => r.GetSubstanceTargetExposure(targetUnit.Target, substances.Single(r => r.Code == codeFocalSubstance)))
                .Cast<SubstanceTargetExposurePattern>()
                .Single();

            var exposure = substanceTargetExposurePattern.SteadyStateTargetExposure;
            Assert.AreEqual(expectedSteadyState, exposure, 1e-4);
        }

        private static List<IExternalIndividualExposure> createFakeExternalIndividualExposures(
            int seed,
            List<Compound> substances,
            double intake,
            List<ExposurePathType> routes
        ) {
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(1, 2, random, useSamplingWeights: false);
            var BW = 70d;
            foreach (var individual in individuals) {
                individual.BodyWeight = BW;
            }
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(individuals);
            var individualExposures = MockExternalExposureGenerator
                .CreateExternalIndividualExposures(individualDays, substances, routes, seed);
            foreach (var item in individualExposures) {
                foreach (var exp in item.ExternalIndividualDayExposures) {
                    var result = new Dictionary<ExposurePathType, ICollection<IIntakePerCompound>>();
                    var intakesPerCompound = new List<AggregateIntakePerCompound> {
                        new AggregateIntakePerCompound() {
                            Compound = substances.First(),
                            Amount = intake * BW,
                        }
                    };
                    result[ExposurePathType.Oral] = intakesPerCompound.Cast<IIntakePerCompound>().ToList();
                    (exp as ExternalIndividualDayExposure).ExposuresPerRouteSubstance = result;
                }
            }

            return individualExposures;
        }

        /// <summary>
        /// Creates a Chlorpyrifos v1 kinetic model instance.
        /// </summary>
        /// <param name="idModelInstance"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        public static KineticModelInstance createFakeModelInstance(
            string idModelInstance,
            List<Compound> substances
        ) {
            var idModelDefinition = "PBK_Chlorpyrifos_V1";
            var modelDefinition = MCRAKineticModelDefinitions.Definitions[idModelDefinition];
            var kineticModelParametersOld = new List<KineticModelInstanceParameter> {
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VLc",
                    Value = 0.0257,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VFc",
                    Value = 0.2142,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VLuc",
                    Value = 0.0076,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VAc",
                    Value = 0.0198,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VVc",
                    Value = 0.0593,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VKc",
                    Value = 0.004,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMc",
                    Value = 0.4,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VUc",
                    Value = 0.0018,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VBrc",
                    Value = 0.02,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VHc",
                    Value = 0.0047,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "QLc",
                    Value = 0.227,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "QFc",
                    Value = 0.052,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "QKc",
                    Value = 0.175,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "QMc",
                    Value = 0.12,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "QUc",
                    Value = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "QBrc",
                    Value = 0.114,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "QHc",
                    Value = 0.04,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "MWP",
                    Value = 350.59,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "MWM1",
                    Value = 334.52,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "MWM2",
                    Value = 198.43,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "LogPP",
                    Value = 4.784,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "LogPM1",
                    Value = 3.894,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "LogPM2",
                    Value = 1.856,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Fa",
                    Value = 0.7,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KaS",
                    Value = 0.00000733,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KaI",
                    Value = 1.00033,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KsI",
                    Value = 0.967749,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "fuP",
                    Value = 0.021,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "fuM1",
                    Value = 0.15,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "fuM2",
                    Value = 0.082,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "BPP",
                    Value = 1.3,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "BPM1",
                    Value = 2.7,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "BPM2",
                    Value = 1,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KurineP",
                    Value = 0,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KurineM1",
                    Value = 0,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KurineM2",
                    Value = 0.026,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "CYPabundanceCYP1A2",
                    Value = 52,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "CYPabundanceCYP2B6",
                    Value = 15.8,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "CYPabundanceCYP2C19",
                    Value = 5.4,
                },
                     new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "CYPabundanceCYP3A4",
                    Value = 137,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "ISEFCYP1A2",
                    Value = 0.072,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "ISEFCYP2B6",
                    Value = 0.476,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "ISEFCYP2C19",
                    Value = 0.209,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "ISEFCYP3A4",
                    Value = 0.107,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "MPL",
                    Value = 32,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP1A2mP1",
                    Value = 3.963,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP2B6mP1",
                    Value = 7.755,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP2C19mP1",
                    Value = 2.744,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP3A4mP1",
                    Value = 17.78,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP1A2P1",
                    Value = 0.61,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP2B6P1",
                    Value = 0.14,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP2C19P1",
                    Value = 1.89,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP3A4P1",
                    Value = 29.77,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP1A2mP2",
                    Value = 2.957,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP2B6mP2",
                    Value = 5.492,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP2C19mP2",
                    Value = 17.51,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP3A4mP2",
                    Value = 23.86,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP1A2P2",
                    Value = 1.25,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP2B6P2",
                    Value = 1.28,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP2C19P2",
                    Value = 1.37,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP3A4P2",
                    Value = 18.13,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMax3c",
                    Value = 37.98,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Km3",
                    Value = 627.9,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMax4c",
                    Value = 1844,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Km4",
                    Value = 289.8,
                },
                    new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "BW",
                    Value = 70,
                }
            };
            var kineticModelParameters = modelDefinition.Parameters
                .Where(r => r.DefaultValue != null)
                .Select(r => new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = r.Id,
                    Value = r.DefaultValue.Value,
                })
                .ToList();

            var kineticModelParametersComplex = modelDefinition.Parameters
                .Where(r => r.DefaultValue == null)
                .SelectMany(c => c.SubstanceParameterValues, (q, r) => {
                    var kmip = new KineticModelInstanceParameter() {
                        IdModelInstance = idModelInstance,
                        Parameter = r.IdParameter,
                        Value = r.DefaultValue.Value,
                    };
                    return kmip;
                })
                .ToList();
            kineticModelParameters.AddRange(kineticModelParametersComplex);

            var kineticModel = new KineticModelInstance() {
                IdModelInstance = idModelInstance,
                KineticModelInstanceParameters = kineticModelParameters.ToDictionary(r => r.Parameter),
                KineticModelDefinition = modelDefinition,
                KineticModelSubstances = substances
                    .Select((s, ix) => new KineticModelSubstance() {
                        Substance = s,
                        SubstanceDefinition = modelDefinition.KineticModelSubstances[ix]
                    })
                    .ToList(),
                IdModelDefinition = idModelDefinition,
                IdTestSystem = "Human",
            };
            return kineticModel;
        }
    }
}
