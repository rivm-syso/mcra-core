using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.FoodSampleFilters {
    public sealed class AnalysedSubstancesFoodSampleFilter : FoodSampleFilterBase {

        /// <summary>
        /// The substances from which the samples should be included.
        /// </summary>
        private readonly HashSet<Compound> _subtances;

        /// <summary>
        /// Initializes a new <see cref="FoodSampleFilterBase"/> instance.
        /// </summary>
        /// <param name="substances"></param>
        public AnalysedSubstancesFoodSampleFilter(ICollection<Compound> substances) : base() {
            _subtances = [.. substances];
        }

        /// <summary>
        /// Implements <see cref="FoodSampleFilterBase.Passes(SampleAnalysis)"/>.
        /// Returns true when the sample contains a substance analysis for any of the substances.
        /// </summary>
        /// <param name="foodSample"></param>
        /// <returns></returns>
        public override bool Passes(FoodSample foodSample) {
            // If no substances filters are specified, then all samples should be included
            var pass = _subtances == null || _subtances.Count == 0;

            if (!pass) {
                //we have substances in the filter, so first check whether any measured concentrations
                //have substances that match the filter
                pass = foodSample.SampleAnalyses.Any(r => r.Concentrations.Keys.Any(c => _subtances.Contains(c)));

                if (!pass) {
                    //no positive measurements for this substance in any of the sample analyses,
                    //check whether the substance is present in any of the corresponding analytical methods
                    var analyticalMethods = foodSample.SampleAnalyses
                        .Select(r => r.AnalyticalMethod)
                        .Where(r => r != null);

                    pass = analyticalMethods.Any(r => r.AnalyticalMethodCompounds.Keys.Any(c => _subtances.Contains(c)));
                }
            }

            return pass;
        }

    }
}
