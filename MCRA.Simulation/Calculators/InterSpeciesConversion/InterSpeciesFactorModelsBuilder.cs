using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.InterSpeciesConversion {
    public sealed class InterSpeciesFactorModelsBuilder {

        private readonly IInterSpeciesFactorModelBuilderSettings _settings;

        public InterSpeciesFactorModelsBuilder(IInterSpeciesFactorModelBuilderSettings settings) {
            _settings = settings;
        }

        /// <summary>
        /// Creates a two-key dictionary of inter-species factor models for specific species/compound
        /// combinations, but also general inter-species factors for substances or species only.
        /// </summary>
        /// <param name="interSpeciesFactors"></param>
        /// <returns></returns>
        public IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> Create(
            ICollection<InterSpeciesFactor> interSpeciesFactors
        ) {
            var results = interSpeciesFactors?
                .Select(c => new InterSpeciesFactorModel(c))
                .ToList();
            results = results ?? new List<InterSpeciesFactorModel>();
            var factor = new InterSpeciesFactor() {
                InterSpeciesFactorGeometricMean = _settings.DefaultInterSpeciesFactorGeometricMean,
                InterSpeciesFactorGeometricStandardDeviation = _settings.DefaultInterSpeciesFactorGeometricStandardDeviation,
                IsDefault = true,
            };
            results.Add(new InterSpeciesFactorModel(factor));
            return results.ToDictionary(r => (r.InterSpeciesFactor.Species, r.InterSpeciesFactor.Compound, r.InterSpeciesFactor.Effect));
        }

        /// <summary>
        /// Retrieves the most specific inter-species factor model from the tuple key dictionary of inter-species factor models.
        /// </summary>
        /// <param name="species"></param>
        /// <param name="substance"></param>
        /// <param name="interSpeciesFactorModels"></param>
        /// <returns></returns>
        public static double GetInterSpeciesFactor(
            IDictionary<(string species, Compound substance, Effect effect), InterSpeciesFactorModel> interSpeciesFactorModels,
            Effect effect,
            string species,
            Compound substance
        ) {
            if (!interSpeciesFactorModels?.Any() ?? true) {
                return 1D;
            } else if (species != null && species.Equals("Human", StringComparison.CurrentCultureIgnoreCase)) {
                return 1D;
            } else if (interSpeciesFactorModels.ContainsKey((species, substance, effect))) {
                return interSpeciesFactorModels[(species, substance, effect)].GeometricMean;
            } else if (interSpeciesFactorModels.ContainsKey((species, substance, null))) {
                return interSpeciesFactorModels[(species, substance, null)].GeometricMean;
            } else if (interSpeciesFactorModels.ContainsKey((species, null, effect))) {
                return interSpeciesFactorModels[(species, null, effect)].GeometricMean;
            } else if (interSpeciesFactorModels.ContainsKey((null, substance, effect))) {
                return interSpeciesFactorModels[(null, substance, effect)].GeometricMean;
            } else if (interSpeciesFactorModels.ContainsKey((species, null, null))) {
                return interSpeciesFactorModels[(species, null, null)].GeometricMean;
            } else if (interSpeciesFactorModels.ContainsKey((null, substance, null))) {
                return interSpeciesFactorModels[(null, substance, null)].GeometricMean;
            } else if (interSpeciesFactorModels.ContainsKey((null, null, effect))) {
                return interSpeciesFactorModels[(null, null, effect)].GeometricMean;
            } else {
                return interSpeciesFactorModels[(null, null, null)].GeometricMean;
            }
            throw new Exception($"Failed to get interspecies factor for species {species}");
        }
    }
}
