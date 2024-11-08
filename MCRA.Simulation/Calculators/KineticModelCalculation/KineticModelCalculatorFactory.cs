using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticConversionFactorModels;
using MCRA.Simulation.Calculators.KineticModelCalculation.LinearDoseAggregationCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.DesolvePbkModelCalculators.ChlorpyrifosPbkModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.DesolvePbkModelCalculators.CosmosKineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.DesolvePbkModelCalculators.KarrerKineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.PbpkModelCalculation.SbmlModelCalculation;

namespace MCRA.Simulation.Calculators.KineticModelCalculation {
    public sealed class KineticModelCalculatorFactory {

        private readonly ICollection<KineticModelInstance> _kineticModelInstances = [];
        private readonly ICollection<IKineticConversionFactorModel> _kineticConversionFactorModels = [];
        private readonly InternalModelType _internalModelType;

        public KineticModelCalculatorFactory(
            ICollection<KineticModelInstance> kineticModelInstances,
            ICollection<IKineticConversionFactorModel> kineticConversionFactorModels,
            ICollection<SimpleAbsorptionFactor> absorptionFactors,
            TargetLevelType targetLevelType,
            InternalModelType internalModelType
        ) {
            _internalModelType = internalModelType;
            if (targetLevelType == TargetLevelType.Systemic) {
                var simpleKineticConversionFactors = absorptionFactors
                    .Select((c, ix) => KineticConversionFactor.FromDefaultAbsorptionFactor(c.ExposureRoute, c.Substance, c.AbsorptionFactor))
                    .ToList();
                _kineticConversionFactorModels = simpleKineticConversionFactors
                    .Select(c => KineticConversionFactorCalculatorFactory.Create(c, false))
                    .ToList();
            } else {
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


        public IKineticModelCalculator CreateHumanKineticModelCalculator(Compound substance) {

            // First, check if there is a PBK model instance available
            var instances = _kineticModelInstances?
                .Where(r => r.IsHumanModel && r.Substances.Contains(substance))
                .ToList();
            if (instances?.Count > 0) {
                if (instances.Count > 1) {
                    throw new Exception($"Multiple kinetic model instances found for {substance.Name}({substance.Code}).");
                }
                if (double.IsNaN(substance.MolecularMass)) {
                    throw new Exception($"Molecular mass is missing for {substance.Name}({substance.Code}), required for PBK modelling.");
                }
                var modelInstance = instances.First();
                if (modelInstance.InputSubstance == substance) {
                    // Only add the instance for the index substance
                    switch (modelInstance.KineticModelType) {
                        case KineticModelType.DeSolve:
                            return new CosmosKineticModelCalculator(modelInstance);
                        case KineticModelType.EuroMix_Bisphenols_PBPK_model_V1:
                            return new KarrerKineticModelCalculator(modelInstance);
                        case KineticModelType.EuroMix_Bisphenols_PBPK_model_V2:
                            return new KarrerReImplementedKineticModelCalculator(modelInstance);
                        case KineticModelType.PBK_Chlorpyrifos_V1:
                            return new ChlorpyrifosPbkModelCalculator(modelInstance);
                        case KineticModelType.SBML:
                            return new SbmlPbkModelCalculator(modelInstance);
                        default:
                            throw new Exception($"No calculator for kinetic model code {modelInstance.IdModelDefinition}");
                    }
                }
            }

            if (_internalModelType == InternalModelType.PBKModelOnly) {
                throw new Exception($"For substance: {substance.Code} no PBK model is found, use option 'PBK models with fallback on kinetic conversion factors'.");
            }

            // No PBK model found, create kinetic conversion factor model calculator
            var conversionFactorModels = _kineticConversionFactorModels?
                .Where(r => r.MatchesFromSubstance(substance))
                .ToList() ?? [];
            return new LinearDoseAggregationCalculator(
                substance,
                conversionFactorModels
            );
        }

        public IDictionary<Compound, IKineticModelCalculator> CreateHumanKineticModels(
            ICollection<Compound> substances
        ) {
            var result = new Dictionary<Compound, IKineticModelCalculator>();
            foreach (var substance in substances) {
                var calculator = CreateHumanKineticModelCalculator(substance);
                if (calculator != null) {
                    result.Add(substance, calculator);
                }
            }
            return result;
        }
    }
}
