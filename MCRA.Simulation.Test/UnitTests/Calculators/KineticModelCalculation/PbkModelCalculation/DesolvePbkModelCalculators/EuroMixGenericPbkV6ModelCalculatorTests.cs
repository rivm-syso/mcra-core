using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.KineticModelCalculation.PbkModelCalculation.DesolvePbkModelCalculators {

    /// <summary>
    /// KineticModelCalculation calculator
    /// </summary>
    [TestClass]
    public class EuroMixGenericPbkV6ModelCalculatorTests : DesolvePbkModelCalculatorBaseTests {

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
        /// Creates a COSMOS v6 model instance. Parameters are set according to the
        /// CompoundV model instance parameters from the EuroMix project.
        /// </summary>
        /// <param name="compound"></param>
        /// <returns></returns>
        private static KineticModelInstance createFakeModelInstance(Compound substance) {
            var idModelDefinition = "EuroMix_Generic_PBTK_model_V6";
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
                    DistributionType = ProbabilityDistribution.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVRich",
                    Value = 0.105,
                    DistributionType = ProbabilityDistribution.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVLiver",
                    Value = 0.024,
                    DistributionType = ProbabilityDistribution.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVBlood",
                    Value = 0.068,
                    DistributionType = ProbabilityDistribution.LogisticNormal,
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
                    DistributionType = ProbabilityDistribution.LogNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFFat",
                    Value = 0.085,
                    DistributionType = ProbabilityDistribution.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFPoor",
                    Value = 0.12,
                    DistributionType = ProbabilityDistribution.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFLiver",
                    Value = 0.27,
                    DistributionType = ProbabilityDistribution.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFSkin",
                    Value = 0.05,
                    DistributionType = ProbabilityDistribution.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Falv",
                    Value = 2220,
                    DistributionType = ProbabilityDistribution.LogNormal,
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
                    Value =49.89895,
                    DistributionType = ProbabilityDistribution.LogNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCPoor",
                    Value = 0.9498036,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCRich",
                    Value = 3.59664,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCLiver",
                    Value = 4.806648,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCSkin",
                    Value = 6.685894,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCSkin_sc",
                    Value = 6.685894,
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
                    DistributionType = ProbabilityDistribution.LogNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Michaelis",
                    Value = 0,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Vmax",
                    Value = 0,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Km",
                    Value = 0,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "CLH",
                    Value = 22.90981,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "fub",
                    Value = 0.11,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Frac",
                    Value = 1,
                    DistributionType = ProbabilityDistribution.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "kGut",
                    Value = 1,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "fSA_exposed",
                    Value = 1,
                }
            };
            var modelDefinition = MCRAKineticModelDefinitions.Definitions[idModelDefinition];
            var kineticModel = new KineticModelInstance() {
                IdModelInstance = idModelInstance,
                IdModelDefinition = idModelDefinition,
                KineticModelSubstances = [
                    new KineticModelSubstance() {
                        Substance = substance
                    }
                ],
                KineticModelInstanceParameters = kineticModelParameters.ToDictionary(r => r.Parameter),
                KineticModelDefinition = modelDefinition,
                IdTestSystem = "Human",
            };
            return kineticModel;
        }
    }
}
