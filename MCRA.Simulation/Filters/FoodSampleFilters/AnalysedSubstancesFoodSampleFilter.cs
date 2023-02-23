using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Filters.FoodSampleFilters {
    public sealed class AnalysedSubstancesFoodSampleFilter : FoodSampleFilterBase {

        /// <summary>
        /// The substances from which the samples should be included.
        /// </summary>
        public HashSet<Compound> Substances { get; set; }

        /// <summary>
        /// Initializes a new <see cref="FoodSampleFilterBase"/> instance.
        /// </summary>
        /// <param name="substances"></param>
        public AnalysedSubstancesFoodSampleFilter(ICollection<Compound> substances) : base() {
            Substances = substances.ToHashSet();
        }

        /// <summary>
        /// Implements <see cref="FoodSampleFilterBase.Passes(SampleAnalysis)"/>.
        /// Returns true when the sample contains a substance analysis for any of the substances.
        /// </summary>
        /// <param name="foodSample"></param>
        /// <returns></returns>
        public override bool Passes(FoodSample foodSample) {
            if (Substances?.Any() ?? false) {
                var analyticalMethods = foodSample.SampleAnalyses.Select(r => r.AnalyticalMethod).Where(r => r != null).ToList();
                return Substances.Any(c => foodSample.SampleAnalyses.SelectMany(r => r.Concentrations.Keys).Contains(c)
                            || (analyticalMethods.Any() ? analyticalMethods.SelectMany(r => r.AnalyticalMethodCompounds.Keys).Contains(c) : false));
            }
            // If no substances filters are specified, then all samples should be included
            return true;
        }

    }
}
