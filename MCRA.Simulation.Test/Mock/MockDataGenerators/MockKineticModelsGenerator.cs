using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.LinearDoseAggregationCalculation;
using MCRA.Simulation.Test.Mock.MockCalculatorSettings;
using System.Collections.Generic;
using System.Linq;

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
        public static TwoKeyDictionary<ExposureRouteType, Compound, double> CreateAbsorptionFactors(
            ICollection<Compound> substances,
            double value
        ) {
            var absorptionFactors = new TwoKeyDictionary<ExposureRouteType, Compound, double>();
            foreach (var substance in substances) {
                absorptionFactors[ExposureRouteType.Dietary, substance] = value;
                absorptionFactors[ExposureRouteType.Dermal, substance] = value;
                absorptionFactors[ExposureRouteType.Oral, substance] = value;
                absorptionFactors[ExposureRouteType.Inhalation, substance] = value;
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
        public static TwoKeyDictionary<ExposureRouteType, Compound, double> CreateAbsorptionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRouteType> routes,
            double value
        ) {
            var absorptionFactors = new TwoKeyDictionary<ExposureRouteType, Compound, double>();
            foreach (var substance in substances) {
                foreach (var route in routes) {
                    absorptionFactors[route, substance] = value;
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
            TwoKeyDictionary<ExposureRouteType, Compound, double> absorptionFactors
        ) {
            var result = substances.ToDictionary(r => r, r => CreateAbsorptionFactorKineticModelCalculator(
                absorptionFactors
                    .Where(a => a.Key.Item2 == r)
                    .ToDictionary(a => a.Key.Item1, a => a.Value)
                ));
            return result;
        }

        public static IKineticModelCalculator CreateAbsorptionFactorKineticModelCalculator(
            Dictionary<ExposureRouteType, double> absorptionFactors
        ) {
            return new LinearDoseAggregationCalculator(absorptionFactors);
        }

        /// <summary>
        /// Creates a dictionary with kinetic model calculators for each substance
        /// </summary>
        /// <param name="substances"></param>
        /// <param name="absorptionFactors"></param>
        /// <returns></returns>
        public static Dictionary<Compound, IKineticModelCalculator> CreateCosmosKineticModelCalculators(
            ICollection<Compound> substances,
            TwoKeyDictionary<ExposureRouteType, Compound, double> absorptionFactors
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
        /// <param name="compound"></param>
        /// <returns></returns>
        public static KineticModelInstance CreateFakeEuroMixPBTKv5KineticModelInstance(Compound compound) {
            var idModelDefinition = "EuroMix_Generic_PBTK_model_V5";
            var idModelInstance = $"{idModelDefinition}-{compound.Code}";
            var kineticModelParameters = new List<KineticModelInstanceParameter> {
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "BM",
                    Value = 70,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "BSA",
                    Value = 190,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVFat",
                    Value = 0.209,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVRich",
                    Value = 0.105,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVLiver",
                    Value = 0.024,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVBlood",
                    Value = 0.068,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Height_sc",
                    Value = 0.0001,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Height_vs",
                    Value = 0.0122,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFBlood",
                    Value = 4.8,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFFat",
                    Value = 0.046,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFPoor",
                    Value = 0.134,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFLiver",
                    Value = 0.259,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFSkin",
                    Value = 0.054,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Falv",
                    Value = 2220,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "mic",
                    Value = 52.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCAir",
                    Value = 1e99,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCFat",
                    Value = 31.11,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCPoor",
                    Value = 3.03,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCRich",
                    Value = 1.92,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCLiver",
                    Value = 4.95,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCSkin",
                    Value = 3.71,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCSkin_sc",
                    Value = 0.1,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Kp_sc_vs",
                    Value = 0.1,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Ke",
                    Value = 7.5,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Michaelis",
                    Value = 1,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Vmax",
                    Value = 0.26,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Km",
                    Value = 0.00484,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "CLH",
                    Value = 0,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "fup",
                    Value = 0.11,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Frac",
                    Value = 1,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.3,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "kGut",
                    Value = 1.3,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "fSA_exposed",
                    Value = 1,
                }
            };
            var kineticModel = new KineticModelInstance() {
                IdModelInstance = idModelInstance,
                KineticModelInstanceParameters = kineticModelParameters.ToDictionary(r => r.Parameter),
                KineticModelDefinition = MCRAKineticModelDefinitions.Definitions[idModelDefinition],
                Substance = compound,
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
        public static KineticModelInstance CreateFakeEuroMixPBTKv6KineticModelInstance(Compound compound) {
            var idModelDefinition = KineticModelType.EuroMix_Generic_PBTK_model_V6.ToString();
            var idModelInstance = $"{idModelDefinition}-{compound.Code}";
            var kineticModelParameters = new List<KineticModelInstanceParameter> {
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "BM",
                    Value = 70,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "BSA",
                    Value = 190,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVFat",
                    Value = 0.209,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVRich",
                    Value = 0.105,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVLiver",
                    Value = 0.024,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scVBlood",
                    Value = 0.068,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Height_sc",
                    Value = 0.0001,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Height_vs",
                    Value = 0.0122,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFBlood",
                    Value = 4.8,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFFat",
                    Value = 0.085,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFPoor",
                    Value = 0.12,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFLiver",
                    Value = 0.27,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "scFSkin",
                    Value = 0.05,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Falv",
                    Value = 2220,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "mic",
                    Value = 52.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCAir",
                    Value = 1e99,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCFat",
                    Value =49.89895,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCPoor",
                    Value = 0.9498036,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCRich",
                    Value = 3.59664,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCLiver",
                    Value = 4.806648,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCSkin",
                    Value = 6.685894,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "PCSkin_sc",
                    Value = 6.685894,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Kp_sc_vs",
                    Value = 0,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Ke",
                    Value = 7.5,
                    DistributionTypeString = "LogNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Michaelis",
                    Value = 0,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Vmax",
                    Value = 0,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Km",
                    Value = 0,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "CLH",
                    Value = 22.90981,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "fub",
                    Value = 0.11,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Frac",
                    Value = 1,
                    DistributionTypeString = "LogisticNormal",
                    CvVariability = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "kGut",
                    Value = 1,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "fSA_exposed",
                    Value = 1,
                }
            };
            var kineticModel = new KineticModelInstance() {
                IdModelInstance = idModelInstance,
                IdModelDefinition = idModelDefinition,
                KineticModelInstanceParameters = kineticModelParameters.ToDictionary(r => r.Parameter),
                KineticModelDefinition = MCRAKineticModelDefinitions.Definitions[idModelDefinition],
                Substance = compound,
                IdTestSystem = "Human",
            };
            return kineticModel;
        }

        private static KineticModelInstance createKineticModelInstance(string idModel, Compound substance, string idModelDefinition) {
            var kineticModelParameters = new List<KineticModelInstanceParameter>();
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "BM",
                Value = 70,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "BSA",
                Value = 190,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "scVFat",
                Value = 0.209,
                DistributionTypeString = "LogisticNormal",
                CvVariability = 0.2,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "scVRich",
                Value = 0.105,
                DistributionTypeString = "LogisticNormal",
                CvVariability = 0.2,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "scVLiver",
                Value = 0.024,
                DistributionTypeString = "LogisticNormal",
                CvVariability = 0.2,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "scVBlood",
                Value = 0.068,
                DistributionTypeString = "LogisticNormal",
                CvVariability = 0.2,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "Height_sc",
                Value = 0.0001,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "Height_vs",
                Value = 0.0122,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "scFBlood",
                Value = 4.8,
                DistributionTypeString = "LogNormal",
                CvVariability = 0.2,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "scFFat",
                Value = 0.085,
                DistributionTypeString = "LogisticNormal",
                CvVariability = 0.2,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "scFPoor",
                Value = 0.12,
                DistributionTypeString = "LogisticNormal",
                CvVariability = 0.2,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "scFLiver",
                Value = 0.27,
                DistributionTypeString = "LogisticNormal",
                CvVariability = 0.2,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "scFSkin",
                Value = 0.05,
                DistributionTypeString = "LogisticNormal",
                CvVariability = 0.2,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "Falv",
                Value = 2220,
                DistributionTypeString = "LogNormal",
                CvVariability = 0.2,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "mic",
                Value = 52.2,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "PCAir",
                Value = 1e99,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "PCFat",
                Value = 31.094,
                DistributionTypeString = "LogNormal",
                CvVariability = 0.2,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "log_aPoor",
                Value = -2.33,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "log_aRich",
                Value = -2.79,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "log_aLiver",
                Value = -1.84,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "log_aSkin",
                Value = -2.13,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "log_aSkin_sc",
                Value = -5.56,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "Kp_sc_vs",
                Value = 0.1,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "Ke",
                Value = 7.5,
                DistributionTypeString = "LogNormal",
                CvVariability = 0.2,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "Michaelis",
                Value = 1,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "Vmax",
                Value = 0.26,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "Km",
                Value = 0.00484,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "CLH",
                Value = 0,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "fup",
                Value = 0.11,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "Frac",
                Value = 0.3,
                DistributionTypeString = "LogisticNormal",
                CvVariability = 0.2,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "kGut",
                Value = 1.3,
            });
            kineticModelParameters.Add(new KineticModelInstanceParameter() {
                IdModelInstance = idModel,
                Parameter = "fSA_exposed",
                Value = 1,
            });
            var kineticModel = new KineticModelInstance() {
                IdModelInstance = idModel,
                KineticModelInstanceParameters = kineticModelParameters.ToDictionary(r => r.Parameter),
                KineticModelDefinition = MCRAKineticModelDefinitions.Definitions[idModel],
                Substance = substance,
                IdModelDefinition = idModelDefinition,
                IdTestSystem = "Human",
            };
            return kineticModel;
        }

        /// <summary>
        /// Creates a COSMOS v4 kinetic model instance
        /// </summary>
        /// <param name="compound"></param>
        /// <returns></returns>
        public static KineticModelInstance CreateFakeChlorpyrifosKineticModelInstance(Compound compound) {
            var idModelDefinition = "PBK_Chlorpyrifos_V1";
            var idModelInstance = $"{idModelDefinition}-{compound.Code}";
            var kineticModelParameters = new List<KineticModelInstanceParameter> {
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VLc",
                    Value = 0.0257,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VFc",
                    Value = 0.2142,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VLuc",
                    Value = 0.0076,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VAc",
                    Value = 0.0198,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VVc",
                    Value = 0.0593,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VKc",
                    Value = 0.004,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMc",
                    Value = 0.4,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VUc",
                    Value = 0.0018,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VBrc",
                    Value = 0.02,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VHc",
                    Value = 0.0047,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "QLc",
                    Value = 0.227,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "QFc",
                    Value = 0.052,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "QKc",
                    Value = 0.175,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "QMc",
                    Value = 0.12,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "QUc",
                    Value = 0.2,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "QBrc",
                    Value = 0.114,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "QHc",
                    Value = 0.04,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "MWP",
                    Value = 350.59,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "MWM1",
                    Value = 334.52,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "MWM2",
                    Value = 198.43,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "LogPP",
                    Value = 4.784,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "LogPM1",
                    Value = 3.894,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "LogPM2",
                    Value = 1.856,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Fa",
                    Value = 0.7,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KaS",
                    Value = 0.00000733,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KaI",
                    Value = 1.00033,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KsI",
                    Value = 0.967749,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "fuP",
                    Value = 0.021,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "fuM1",
                    Value = 0.15,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "fuM2",
                    Value = 0.082,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "BPP",
                    Value = 1.3,
                },
                new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "BPM1",
                    Value = 2.7,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "BPM2",
                    Value = 1,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KurineP",
                    Value = 0,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KurineM1",
                    Value = 0,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KurineM2",
                    Value = 0.026,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "CYPabundanceCYP1A2",
                    Value = 52,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "CYPabundanceCYP2B6",
                    Value = 15.8,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "CYPabundanceCYP2C19",
                    Value = 5.4,
                },
                     new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "CYPabundanceCYP3A4",
                    Value = 137,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "ISEFCYP1A2",
                    Value = 0.072,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "ISEFCYP2B6",
                    Value = 0.476,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "ISEFCYP2C19",
                    Value = 0.209,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "ISEFCYP3A4",
                    Value = 0.107,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "MPL",
                    Value = 32,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP1A2mP1",
                    Value = 3.963,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP2B6mP1",
                    Value = 7.755,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP2C19mP1",
                    Value = 2.744,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP3A4mP1",
                    Value = 17.78,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP1A2P1",
                    Value = 0.61,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP2B6P1",
                    Value = 0.14,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP2C19P1",
                    Value = 1.89,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP3A4P1",
                    Value = 29.77,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP1A2mP2",
                    Value = 2.957,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP2B6mP2",
                    Value = 5.492,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP2C19mP2",
                    Value = 17.51,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMaxCYP3A4mP2",
                    Value = 23.86,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP1A2P2",
                    Value = 1.25,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP2B6P2",
                    Value = 1.28,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP2C19P2",
                    Value = 1.37,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "KmCYP3A4P2",
                    Value = 18.13,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMax3c",
                    Value = 37.98,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Km3",
                    Value = 627.9,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "VMax4c",
                    Value = 1844,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "Km4",
                    Value = 289.8,
                },
                    new KineticModelInstanceParameter() {
                    IdModelInstance = idModelInstance,
                    Parameter = "BW",
                    Value = 70,
                }
            };
            var kineticModel = new KineticModelInstance() {
                IdModelInstance = idModelInstance,
                KineticModelInstanceParameters = kineticModelParameters.ToDictionary(r => r.Parameter),
                KineticModelDefinition = MCRAKineticModelDefinitions.Definitions[idModelDefinition],
                Substance = compound,
                IdModelDefinition = idModelDefinition,
                IdTestSystem = "Human",
            };
            return kineticModel;
        }
    }
}
