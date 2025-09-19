using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.PbpkModelCalculation;
using MCRA.Simulation.Calculators.PbpkModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation;

namespace MCRA.Simulation.Test.UnitTests.Calculators.PbkModelCalculation.DesolvePbkModelCalculators {

    /// <summary>
    /// KineticModelCalculation calculator
    /// </summary>
    [TestClass]
    public class EuroMixGenericPbkV5ModelCalculatorTests : DesolvePbkModelCalculatorBaseTests {

        protected override KineticModelInstance getDefaultInstance(params Compound[] substance) {
            var instance = createFakeModelInstance(substance.Single());
            return instance;
        }

        protected override PbkModelCalculatorBase createCalculator(
            KineticModelInstance instance,
            PbkSimulationSettings simulationSettings
        ) {
            var calculator = new CosmosKineticModelCalculator(instance, simulationSettings);
            return calculator;
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
        /// Creates a COSMOS v5 kinetic model instance.
        /// </summary>
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
            var kineticModel = new KineticModelInstance() {
                IdModelInstance = idModelInstance,
                KineticModelInstanceParameters = kineticModelParameters.ToDictionary(r => r.Parameter),
                KineticModelDefinition = MCRAKineticModelDefinitions.Definitions[idModelDefinition],
                KineticModelSubstances = [new() { Substance = substance }],
                IdModelDefinition = idModelDefinition,
                IdTestSystem = "Human",
            };
            return kineticModel;
        }
    }
}
