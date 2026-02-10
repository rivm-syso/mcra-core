using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.KineticModelDefinitions;
using MCRA.Simulation.Calculators.KineticConversionCalculation;
using MCRA.Simulation.Calculators.PbkModelCalculation;
using MCRA.Simulation.Calculators.PbkModelCalculation.ReverseDoseCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.PbkModelCalculation.ReverseDoseCalculation {

    [TestClass]
    public class ReverseDoseCalculatorTests {

        /// <summary>
        /// PBK model: calculates reverse dose based on PBK model.
        /// </summary>
        [TestMethod]
        [DataRow(ExposureType.Chronic, PbkModelType.SBML)]
        [DataRow(ExposureType.Acute, PbkModelType.SBML)]
        [DataRow(ExposureType.Chronic, PbkModelType.DeSolve)]
        [DataRow(ExposureType.Acute, PbkModelType.DeSolve)]
        public void PbkModelCalculator_TestReverse(ExposureType exposureType, PbkModelType modelType) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var substances = FakeSubstancesGenerator.Create(1);
            var substance = substances.First();
            var individual = FakeIndividualsGenerator.CreateSingle();

            var internalDose = 10d;
            var internalDoseUnit = TargetUnit.FromInternalDoseUnit(DoseUnit.ugPerL, BiologicalMatrix.Liver);
            var externalExposuresUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.mgPerKgBWPerDay);

            var simulationSettings = new PbkSimulationSettings {
                NumberOfSimulatedDays = 10,
                UseRepeatedDailyEvents = true,
                PrecisionReverseDoseCalculation = 0.05
            };
            var instance = modelType switch {
                PbkModelType.SBML => createFakeSbmlPbkModelCalculator(substance, simulationSettings),
                PbkModelType.DeSolve => createFakeDesolveModelInstance(substance),
                _ => throw new NotImplementedException($"Model type {modelType} not implemented"),
            };
            var calculator = PbkModelCalculatorFactory.Create(instance, simulationSettings);

            var reverseDoseCalculator = new ReverseDoseCalculator();
            var externalDose = reverseDoseCalculator
                .Reverse(
                    calculator,
                    individual,
                    internalDose,
                    internalDoseUnit,
                    ExposureRoute.Oral,
                    externalExposuresUnit.ExposureUnit,
                    exposureType,
                    random
                );

            var forwardDoseCalculator = new PbkKineticConversionCalculator(calculator);
            var resultForward = forwardDoseCalculator
                .Forward(
                    individual,
                    externalDose,
                    ExposureRoute.Oral,
                    externalExposuresUnit.ExposureUnit,
                    internalDoseUnit,
                    exposureType,
                    random
                );

            Assert.AreEqual(internalDose, resultForward, 1e-1);
        }

        private static KineticModelInstance createFakeSbmlPbkModelCalculator(
            Compound substance,
            PbkSimulationSettings simulationSettings
        ) {
            var filename = "Resources/PbkModels/EuroMixGenericPbk.sbml";
            var modelDefinition = SbmlPbkModelSpecificationBuilder
                .CreateFromSbmlFile(filename);
            var instance = new KineticModelInstance() {
                KineticModelDefinition = modelDefinition,
                ModelSubstances = [
                     new PbkModelSubstance() {
                         Substance = substance
                     }
                ],
                KineticModelInstanceParameters = new Dictionary<string, KineticModelInstanceParameter>(),
                IdTestSystem = "Human",
                IdModelDefinition = modelDefinition.Id,
            };

            return instance;
        }

        private static KineticModelInstance createFakeDesolveModelInstance(
            Compound substance
        ) {
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
                    Value = 0.085,
                    DistributionType = PbkModelParameterDistributionType.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFPoor",
                    Value = 0.12,
                    DistributionType = PbkModelParameterDistributionType.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFLiver",
                    Value = 0.27,
                    DistributionType = PbkModelParameterDistributionType.LogisticNormal,
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFSkin",
                    Value = 0.05,
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
                    Value =49.89895,
                    DistributionType = PbkModelParameterDistributionType.LogNormal,
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
                    DistributionType = PbkModelParameterDistributionType.LogNormal,
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
                    DistributionType = PbkModelParameterDistributionType.LogisticNormal,
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
            var modelDefinition = McraEmbeddedPbkModelDefinitions.Definitions[idModelDefinition];
            var kineticModel = new KineticModelInstance() {
                IdModelInstance = idModelInstance,
                IdModelDefinition = idModelDefinition,
                ModelSubstances = [
                    new PbkModelSubstance() {
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
