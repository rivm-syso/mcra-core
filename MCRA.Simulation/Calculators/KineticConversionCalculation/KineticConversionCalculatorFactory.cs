using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Calculators.PbkModelCalculation;

namespace MCRA.Simulation.Calculators.KineticConversionCalculation {
    public sealed class KineticConversionCalculatorFactory {

        private readonly ICollection<KineticModelInstance> _kineticModelInstances = [];
        private readonly ICollection<IKineticConversionFactorModel> _kineticConversionFactorModels = [];
        private readonly InternalModelType _internalModelType;

        private readonly ICollection<ExposureRoute> _exposureRoutes;
        private readonly IDictionary<(ExposureRoute, Compound), SimpleAbsorptionFactor> _absorptionFactorsLookup;

        public KineticConversionCalculatorFactory(
            ICollection<KineticModelInstance> kineticModelInstances,
            ICollection<IKineticConversionFactorModel> kineticConversionFactorModels,
            ICollection<SimpleAbsorptionFactor> absorptionFactors,
            TargetLevelType targetLevelType,
            InternalModelType internalModelType,
            ICollection<ExposureRoute> exposureRoutes
        ) {
            _absorptionFactorsLookup = absorptionFactors?.ToDictionary(r => (r.ExposureRoute, r.Substance)) ?? [];
            _exposureRoutes = exposureRoutes;
            if (targetLevelType == TargetLevelType.Systemic) {
                // For systemic target level, always use conversion factor models based on absorption factors
                _internalModelType = InternalModelType.ConversionFactorModel;
                var simpleKineticConversionFactors = absorptionFactors
                    .Select((c, ix) => KineticConversionFactor.FromDefaultAbsorptionFactor(c.ExposureRoute, c.Substance, c.AbsorptionFactor))
                    .ToList();
                _kineticConversionFactorModels = [.. simpleKineticConversionFactors.Select(c => KineticConversionFactorCalculatorFactory.Create(c, false))];
            } else {
                _internalModelType = internalModelType;
                if (internalModelType == InternalModelType.ConversionFactorModel
                    || internalModelType == InternalModelType.PBKModel
                ) {
                    _kineticModelInstances = kineticModelInstances;
                    _kineticConversionFactorModels = kineticConversionFactorModels;
                } else if (internalModelType == InternalModelType.PBKModelOnly) {
                    _kineticModelInstances = kineticModelInstances;
                }
            }
        }

        public KineticConversionCalculatorFactory(
            ICollection<KineticModelInstance> kineticModelInstances,
            ICollection<SimpleAbsorptionFactor> absorptionFactors,
            ICollection<ExposureRoute> exposureRoutes
        ) {
            _kineticModelInstances = kineticModelInstances;
            _absorptionFactorsLookup = absorptionFactors.ToDictionary(r => (r.ExposureRoute, r.Substance)) ?? [];
            _exposureRoutes = exposureRoutes;
        }

        public IKineticConversionCalculator CreateHumanKineticModelCalculator(
            Compound substance,
            PbkSimulationSettings simulationSettings
        ) {
            // First, check if there is a PBK model instance available
            // Note molecular weight is not needed for PBK model (and also linear dose aggregation model)
            var instances = _kineticModelInstances?
                .Where(r => r.IsHumanModel && r.Substances.Contains(substance))
                .ToList();
            if (instances?.Count > 0) {
                if (instances.Count > 1) {
                    throw new Exception($"Multiple kinetic model instances found for {substance.Name}({substance.Code}).");
                }

                var modelInstance = instances.First();
                if (modelInstance.InputSubstance == substance) {
                    var substanceAbsorptionFactors = _exposureRoutes.ToDictionary(
                        r => r,
                        r => _absorptionFactorsLookup.TryGetValue((r, substance), out var valSpecific)
                            ? valSpecific.AbsorptionFactor
                            : _absorptionFactorsLookup.TryGetValue((r, null), out var valGeneric) ? valGeneric.AbsorptionFactor : 1D
                    );

                    return new PbkKineticConversionCalculator(
                        modelInstance,
                        simulationSettings,
                        substanceAbsorptionFactors
                    );
                }
            }

            if (_internalModelType == InternalModelType.PBKModelOnly) {
                throw new Exception($"No PBK model found for substance [{substance.Code}].");
            }

            // No PBK model found, create kinetic conversion factor model calculator
            var conversionFactorModels = _kineticConversionFactorModels?
                .Where(r => r.SubstanceFrom == substance || r.SubstanceFrom == null)
                .ToList() ?? [];
            return new LinearDoseAggregationCalculator(
                substance,
                conversionFactorModels
            );
        }

        public IDictionary<Compound, IKineticConversionCalculator> CreateHumanKineticModels(
            ICollection<Compound> substances,
            PbkSimulationSettings simulationSettings
        ) {
            var result = new Dictionary<Compound, IKineticConversionCalculator>();
            foreach (var substance in substances) {
                var calculator = CreateHumanKineticModelCalculator(substance, simulationSettings);
                if (calculator != null) {
                    result.Add(substance, calculator);
                }
            }
            return result;
        }
    }
}
