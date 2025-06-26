using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.FoodSampleFilters {
    public sealed class SampleMonthsFilter : FoodSampleFilterBase {

        /// <summary>
        /// The period ranges from which the samples should be included.
        /// </summary>
        private readonly HashSet<int> _months;

        /// <summary>
        /// Specifies whether samples with unspecified sampling dates should be included or not.
        /// </summary>
        private readonly bool _includeUnspecifiedSamplingDates;

        /// <summary>
        /// Initializes a new <see cref="SampleMonthsFilter"/> instance.
        /// </summary>
        /// <param name="months"></param>
        /// <param name="includeUnspecifiedSamplingDates"></param>
        public SampleMonthsFilter(
            List<int> months,
            bool includeUnspecifiedSamplingDates
        ) : base() {
            _months = months.ToHashSet();
            _includeUnspecifiedSamplingDates = includeUnspecifiedSamplingDates;
        }

        /// <summary>
        /// Implements <see cref="FoodSampleFilterBase.Passes(FoodSample)"/>.
        /// Returns true when the sample is in one of the specified sampling periods.
        /// If no periods filters are specified, then all samples should be included.
        /// </summary>
        /// <param name="foodSample"></param>
        /// <returns></returns>
        public override bool Passes(FoodSample foodSample) {
            if (_months?.Count > 0) {
                if (foodSample.DateSampling == null) {
                    // If sampling date not specified, return default value for missing sampling dates.
                    return _includeUnspecifiedSamplingDates;
                } else {
                    // Return true if the month of sampling is one of the selected months.
                    return _months.Contains(((DateTime)foodSample.DateSampling).Month);
                }
            }
            // If no month filters are specified, then all samples should be included
            return true;
        }
    }
}
