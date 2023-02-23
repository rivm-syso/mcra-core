using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Filters.FoodSampleFilters {
    public sealed class SampleMonthsFilter : FoodSampleFilterBase {

        /// <summary>
        /// The period ranges from which the samples should be included.
        /// </summary>
        public List<int> Months { get; set; }

        /// <summary>
        /// Specifies whether samples with unspecified sampling dates should be included or not.
        /// </summary>
        public bool IncludeUnspecifiedSamplingDates { get; set; } = true;

        /// <summary>
        /// Initializes a new <see cref="SampleMonthsFilter"/> instance.
        /// </summary>
        /// <param name="months"></param>
        /// <param name="includeUnspecifiedSamplingDates"></param>
        public SampleMonthsFilter(
            List<int> months,
            bool includeUnspecifiedSamplingDates
        ) : base() {
            Months = months;
            IncludeUnspecifiedSamplingDates = includeUnspecifiedSamplingDates;
        }

        /// <summary>
        /// Implements <see cref="FoodSampleFilterBase.Passes(FoodSample)"/>.
        /// Returns true when the sample is in one of the specified sampling periods.
        /// If no periods filters are specified, then all samples should be included. 
        /// </summary>
        /// <param name="foodSample"></param>
        /// <returns></returns>
        public override bool Passes(FoodSample foodSample) {
            if (Months?.Any() ?? false) {
                if (foodSample.DateSampling == null) {
                    // If sampling date not specified, return default value for missing sampling dates.
                    return IncludeUnspecifiedSamplingDates;
                } else {
                    // Return true if the month of sampling is one of the selected months.
                    return Months.Any(r => ((DateTime)foodSample.DateSampling).Month == r);
                }
            }
            // If no month filters are specified, then all samples should be included

            return true;
        }
    }
}
