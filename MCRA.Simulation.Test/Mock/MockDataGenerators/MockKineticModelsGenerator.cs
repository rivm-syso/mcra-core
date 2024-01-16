using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.LinearDoseAggregationCalculation;
using MCRA.Simulation.Test.Mock.MockCalculatorSettings;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock kinetic models
    /// </summary>
    public static class MockKineticModelsGenerator {

        /// <summary>
        /// Creates a dictionary with absorption factors for each combination of route and substance
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IDictionary<(ExposurePathType, Compound), double> CreateAbsorptionFactors(
            ICollection<Compound> substances,
            double value
        ) {
            var absorptionFactors = new Dictionary<(ExposurePathType, Compound), double>();
            foreach (var substance in substances) {
                absorptionFactors[(ExposurePathType.Dietary, substance)] = value;
                absorptionFactors[(ExposurePathType.Dermal, substance)] = value;
                absorptionFactors[(ExposurePathType.Oral, substance)] = value;
                absorptionFactors[(ExposurePathType.Inhalation, substance)] = value;
            }
            return absorptionFactors;
        }

        /// <summary>
        /// Creates a dictionary with absorption factors for each combination of route and substance
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="routes"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IDictionary<(ExposurePathType, Compound), double> CreateAbsorptionFactors(
            ICollection<Compound> substances,
            ICollection<ExposurePathType> routes,
            double value
        ) {
            var absorptionFactors = new Dictionary<(ExposurePathType, Compound), double>();
            foreach (var substance in substances) {
                foreach (var route in routes) {
                    absorptionFactors[(route, substance)] = value;
                }
            }
            return absorptionFactors;
        }

        /// <summary>
        /// Creates a dictionary with kinetic model calculators for each substance
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="absorptionFactors"></param>
        /// <returns></returns>
        public static IDictionary<Compound, IKineticModelCalculator> CreateAbsorptionFactorKineticModelCalculators(
            ICollection<Compound> substances,
            IDictionary<(ExposurePathType Route, Compound Substance), double> absorptionFactors
        ) {
            var result = substances.ToDictionary(r => r, r => CreateAbsorptionFactorKineticModelCalculator(
                r,
                absorptionFactors
                    .Where(a => a.Key.Substance == r)
                    .ToDictionary(a => a.Key.Route, a => a.Value)
                ));
            return result;
        }

        public static IKineticModelCalculator CreateAbsorptionFactorKineticModelCalculator(
            Compound substance,
            Dictionary<ExposurePathType, double> absorptionFactors
        ) {
            return new LinearDoseAggregationCalculator(substance, absorptionFactors);
        }

        /// <summary>
        /// Creates a dictionary with kinetic model calculators for each substance
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="absorptionFactors"></param>
        /// <returns></returns>
        public static Dictionary<Compound, IKineticModelCalculator> CreateCosmosKineticModelCalculators(
            ICollection<Compound> substances,
            IDictionary<(ExposurePathType, Compound), double> absorptionFactors
        ) {
            var kineticModelInstances = new List<KineticModelInstance>();
            var settings = new MockKineticModelCalculatorFactorySettings() {
                NumberOfDays = 100,
                NumberOfDosesPerDay = 1,
                CodeModel = "EuroMix_Generic_PBTK_model_V5",
                CodeCompartment = "CLiver",
                NonStationaryPeriod = 10,
            };
            foreach (var substance in substances) {
                var instance = createKineticModelInstance("CosmosV4", substance, settings.CodeModel);
                kineticModelInstances.Add(instance);
            }
            var kineticModelCalculators = new Dictionary<Compound, IKineticModelCalculator>();

            var kineticModelCalculatorFactory = new KineticModelCalculatorFactory(absorptionFactors, kineticModelInstances);
            foreach (var substance in substances) {
                var calculator = kineticModelCalculatorFactory.CreateHumanKineticModelCalculator(substance);
                kineticModelCalculators.Add(substance, calculator);
            }

            return kineticModelCalculators;
        }

        /// <summary>
        /// Creates a COSMOS v4 kinetic model instance
        /// </summary>
        /// <param name="substance"></param>
        /// <returns></returns>
        public static KineticModelInstance CreateFakeEuroMixPBTKv5KineticModelInstance(Compound substance) {
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

        /// <summary>
        /// Creates a COSMOS v6 model instance. Parameters are set according to the
        /// CompoundV model instance parameters from the EuroMix project.
        /// </summary>
        /// <param name="compound"></param>
        /// <returns></returns>
        public static KineticModelInstance CreateFakeEuroMixPBTKv6KineticModelInstance(Compound substance) {
            var idModelDefinition = KineticModelType.EuroMix_Generic_PBTK_model_V6.ToString();
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
                    Value = 0.085,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFPoor",
                    Value = 0.12,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFLiver",
                    Value = 0.27,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFSkin",
                    Value = 0.05,
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
                    Value =49.89895,
                    DistributionTypeString = "LogNormal",
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
                    Value = 0,
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
                    DistributionTypeString = "LogisticNormal",
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
                KineticModelSubstances = new List<KineticModelSubstance>() {
                    new KineticModelSubstance() {
                        Substance = substance
                    }
                },
                KineticModelInstanceParameters = kineticModelParameters.ToDictionary(r => r.Parameter),
                KineticModelDefinition = modelDefinition,
                IdTestSystem = "Human",
            };
            return kineticModel;
        }

        private static KineticModelInstance createKineticModelInstance(string idModel, Compound substance, string idModelDefinition) {
            var kineticModelParameters = new List<KineticModelInstanceParameter> {
                new() {
                    IdModelInstance = idModel,
                    Parameter = "BM",
                    Value = 70,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "BSA",
                    Value = 190,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "scVFat",
                    Value = 0.209,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "scVRich",
                    Value = 0.105,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "scVLiver",
                    Value = 0.024,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "scVBlood",
                    Value = 0.068,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "Height_sc",
                    Value = 0.0001,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "Height_vs",
                    Value = 0.0122,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "scFBlood",
                    Value = 4.8,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "scFFat",
                    Value = 0.085,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "scFPoor",
                    Value = 0.12,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "scFLiver",
                    Value = 0.27,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "scFSkin",
                    Value = 0.05,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "Falv",
                    Value = 2220,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "mic",
                    Value = 52.2,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "PCAir",
                    Value = 1e99,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "PCFat",
                    Value = 31.094,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "log_aPoor",
                    Value = -2.33,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "log_aRich",
                    Value = -2.79,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "log_aLiver",
                    Value = -1.84,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "log_aSkin",
                    Value = -2.13,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "log_aSkin_sc",
                    Value = -5.56,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "Kp_sc_vs",
                    Value = 0.1,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "Ke",
                    Value = 7.5,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "Michaelis",
                    Value = 1,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "Vmax",
                    Value = 0.26,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "Km",
                    Value = 0.00484,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "CLH",
                    Value = 0,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "fup",
                    Value = 0.11,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "Frac",
                    Value = 0.3,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "kGut",
                    Value = 1.3,
                },
                new() {
                    IdModelInstance = idModel,
                    Parameter = "fSA_exposed",
                    Value = 1,
                }
            };
            var kineticModel = new KineticModelInstance() {
                IdModelInstance = idModel,
                KineticModelDefinition = MCRAKineticModelDefinitions.Definitions[idModel],
                KineticModelSubstances = new List<KineticModelSubstance>() {
                    new KineticModelSubstance() {
                        Substance = substance
                    }
                },
                KineticModelInstanceParameters = kineticModelParameters.ToDictionary(r => r.Parameter),
                IdModelDefinition = idModelDefinition,
                IdTestSystem = "Human",
            };
            return kineticModel;
        }

        /// <summary>
        /// Creates a Chlorpyrifos v1 kinetic model instance.
        /// </summary>
        /// <param name="idModelInstance"></param>
        /// <param name="substances"></param>
        /// <returns></returns>
        public static KineticModelInstance CreateFakeChlorpyrifosKineticModelInstance(
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
                }).ToList();
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
