using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation.AbsorptionFactorsGeneration;
using MCRA.Simulation.Calculators.KineticModelCalculation.ChlorpyrifosKineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.CosmosKineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.KarrerKineticModelCalculation;
using MCRA.Simulation.Calculators.KineticModelCalculation.LinearDoseAggregationCalculation;
using MCRA.Utils.Collections;

namespace MCRA.Simulation.Calculators.KineticModelCalculation {
    public sealed class KineticModelCalculatorFactory {

        private readonly IDictionary<(ExposureRouteType, Compound), double> _defaultAbsorptionFactors;
        private readonly ICollection<KineticModelInstance> _kineticModelInstances;

        public KineticModelCalculatorFactory(
            IDictionary<(ExposureRouteType, Compound), double> defaultAbsorptionFactors,
            ICollection<KineticModelInstance> kineticModelInstances
        ) {
            _defaultAbsorptionFactors = defaultAbsorptionFactors;
            _kineticModelInstances = kineticModelInstances;
        }

        public IKineticModelCalculator CreateSpeciesKineticModelCalculator(Compound substance, string species) {
            var absorptionFactors = _defaultAbsorptionFactors.Get(substance);
            if (_kineticModelInstances.Any(c => c.Substances.Contains(substance) && c.IdTestSystem.Equals(species, StringComparison.InvariantCultureIgnoreCase))) {
                if (double.IsNaN(substance.MolecularMass)) {
                    throw new Exception($"Molecular mass is missing for {substance.Code}, required for PBK modelling.");
                }
                var modelInstance = _kineticModelInstances.FirstOrDefault(c => c.Substances.Contains(substance) && c.IdTestSystem.Equals(species, StringComparison.InvariantCultureIgnoreCase));
                switch (modelInstance.KineticModelType) {
                    case KineticModelType.EuroMix_Generic_PBTK_model_V5:
                    case KineticModelType.EuroMix_Generic_PBTK_model_V6:
                        return new CosmosKineticModelCalculator(modelInstance, absorptionFactors);
                    case KineticModelType.EuroMix_Bisphenols_PBPK_model_V1:
                        return new KarrerKineticModelCalculator(modelInstance, absorptionFactors);
                    case KineticModelType.EuroMix_Bisphenols_PBPK_model_V2:
                        return new KarrerReImplementedKineticModelCalculator(modelInstance, absorptionFactors);
                    case KineticModelType.PBK_Chlorpyrifos_V1:
                        return new ChlorpyrifosKineticModelCalculator(modelInstance, absorptionFactors);
                    case KineticModelType.Undefined:
                    default:
                        throw new Exception($"No calculator for kinetic model code {modelInstance.IdModelDefinition}");
                }
            } else {
                return new LinearDoseAggregationCalculator(substance, absorptionFactors);
            }
        }

        public IKineticModelCalculator CreateHumanKineticModelCalculator(Compound substance) {
            var absorptionFactors = _defaultAbsorptionFactors.Get(substance);
            var instances = _kineticModelInstances?
                .Where(r => r.IsHumanModel && r.Substances.Contains(substance))
                .ToList();

            if (instances?.Any() ?? false) {
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
                        case KineticModelType.EuroMix_Generic_PBTK_model_V5:
                        case KineticModelType.EuroMix_Generic_PBTK_model_V6:
                            return new CosmosKineticModelCalculator(modelInstance, absorptionFactors);
                        case KineticModelType.EuroMix_Bisphenols_PBPK_model_V1:
                            return new KarrerKineticModelCalculator(modelInstance, absorptionFactors);
                        case KineticModelType.EuroMix_Bisphenols_PBPK_model_V2:
                            return new KarrerReImplementedKineticModelCalculator(modelInstance, absorptionFactors);
                        case KineticModelType.PBK_Chlorpyrifos_V1:
                            return new ChlorpyrifosKineticModelCalculator(modelInstance, absorptionFactors);
                        case KineticModelType.Undefined:
                        default:
                            throw new Exception($"No calculator for kinetic model code {modelInstance.IdModelDefinition}");
                    }
                }
                return null;
            } else {
                return new LinearDoseAggregationCalculator(substance, absorptionFactors);
            }
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
