﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Calculators.KineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.LinearDoseAggregationCalculation;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating fake kinetic conversion models.
    /// </summary>
    public static class FakeKineticModelsGenerator {

        /// <summary>
        /// Creates a dictionary with absorption factors for each combination of route and substance
        /// </summary>
        public static IDictionary<(ExposureRoute, Compound), double> CreateAbsorptionFactors(
            ICollection<Compound> substances,
            double value
        ) {
            var kineticConversionFactors = new Dictionary<(ExposureRoute, Compound), double>();
            foreach (var substance in substances) {
                kineticConversionFactors[(ExposureRoute.Dermal, substance)] = value;
                kineticConversionFactors[(ExposureRoute.Oral, substance)] = value;
                kineticConversionFactors[(ExposureRoute.Inhalation, substance)] = value;
            }
            return kineticConversionFactors;
        }

        /// <summary>
        /// Creates a dictionary with absorption factors for each combination of route and substance
        /// </summary>
        public static IDictionary<(ExposureRoute, Compound), double> CreateAbsorptionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            double value
        ) {
            var kineticConversionFactors = new Dictionary<(ExposureRoute, Compound), double>();
            foreach (var substance in substances) {
                foreach (var route in routes) {
                    kineticConversionFactors[(route, substance)] = value;
                }
            }
            return kineticConversionFactors;
        }

        /// <summary>
        /// Creates a dictionary with absorption factors for each combination of route and substance
        /// </summary>
        public static ICollection<KineticConversionFactor> CreateKineticConversionFactors(
            ICollection<Compound> substances,
            ICollection<ExposureRoute> routes,
            TargetUnit target
        ) {
            var kineticConversionFactors = new List<KineticConversionFactor>();
            foreach (var substance in substances) {
                foreach (var route in routes) {
                    kineticConversionFactors.Add(new KineticConversionFactor() {
                        SubstanceFrom = substance,
                        SubstanceTo = substance,
                        ExposureRouteFrom = route,
                        DoseUnitFrom = ExposureUnitTriple.FromExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay),
                        ConversionFactor = .5,
                        BiologicalMatrixTo = target.BiologicalMatrix,
                        ExpressionTypeTo = target.ExpressionType,
                        DoseUnitTo = target.ExposureUnit,
                    });
                }
            }
            return kineticConversionFactors;
        }

        public static List<IKineticConversionFactorModel> CreateKineticConversionFactorModels(
            List<Compound> substances,
            List<ExposureRoute> routes,
            TargetUnit target
        ) {
            var kineticConversionFactors = CreateKineticConversionFactors(
                substances,
                routes,
                target
            );
            var kineticConversionFactorModels = kineticConversionFactors
                .Select(c => KineticConversionFactorCalculatorFactory.Create(c, false))
                .ToList();
            return kineticConversionFactorModels;
        }

        /// <summary>
        /// Creates a dictionary with kinetic model calculators for each substance
        /// </summary>
        public static IDictionary<Compound, IKineticModelCalculator> CreateAbsorptionFactorKineticModelCalculators(
            ICollection<Compound> substances,
            IDictionary<(ExposureRoute Route, Compound Substance), double> kineticConversionFactors,
            TargetUnit target,
            ExternalExposureUnit externalExposureUnit = ExternalExposureUnit.ugPerKgBWPerDay
        ) {
            var result = substances
                .ToDictionary(
                    r => r,
                    r => CreateAbsorptionFactorKineticModelCalculator(
                        r,
                        kineticConversionFactors
                            .Where(a => a.Key.Substance == r)
                            .Select(c => new KineticConversionFactor() {
                                SubstanceFrom = r,
                                ExposureRouteFrom = c.Key.Route,
                                DoseUnitFrom = ExposureUnitTriple.FromExposureUnit(externalExposureUnit),
                                ConversionFactor = c.Value,
                                BiologicalMatrixTo = target.BiologicalMatrix,
                                ExpressionTypeTo = target.ExpressionType,
                                DoseUnitTo = target.ExposureUnit
                            })
                            .ToList()
                        )
                    );
            return result;
        }

        /// <summary>
        /// Creates a linear kinetic conversion model calculator instance for the specified
        /// substance based on the provided collection of kinetic conversion factors.
        /// </summary>
        public static IKineticModelCalculator CreateAbsorptionFactorKineticModelCalculator(
            Compound substance,
            ICollection<KineticConversionFactor> kineticConversionFactors
        ) {
            var conversionModels = kineticConversionFactors?
                .Select(c => KineticConversionFactorCalculatorFactory.Create(c, false))
                .ToList();
            return new LinearDoseAggregationCalculator(substance, conversionModels);
        }

        /// <summary>
        /// Creates a COSMOS v6 model instance. Parameters are set according to the
        /// CompoundV model instance parameters from the EuroMix project.
        /// </summary>
        public static KineticModelInstance CreatePbkModelInstance(Compound substance) {
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
                    Value = 0,
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
