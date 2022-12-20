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
            var idModelInstance = $"{idModelDefinition }-{compound.Code}";
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
            var idModelInstance = $"{idModelDefinition }-{compound.Code}";
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
    }
}
