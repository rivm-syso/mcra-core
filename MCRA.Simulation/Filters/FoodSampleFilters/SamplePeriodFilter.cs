using MCRA.Utils.DateTimes;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Filters.FoodSampleFilters {
    public sealed class SamplePeriodFilter : FoodSampleFilterBase {

        /// <summary>
        /// The period ranges from which the samples should be included.
        /// </summary>
        public List<TimeRange> Periods { get; set; }

        /// <summary>
        /// Specifies whether samples with unspecified sampling dates should be included or not.
        /// </summary>
        public bool IncludeUnspecifiedSamplingDates { get; set; } = true;

        /// <summary>
        /// Initializes a new <see cref="SamplePeriodFilter"/> instance.
        /// </summary>
        /// <param name="periods"></param>
        /// <param name="includeUnspecifiedSamplingDates"></param>
        public SamplePeriodFilter(
            List<TimeRange> periods,
            bool includeUnspecifiedSamplingDates
        ) : base() {
            Periods = periods;
            IncludeUnspecifiedSamplingDates = includeUnspecifiedSamplingDates;
        }

        /// <summary>
        /// Implements <see cref="FoodSampleFilterBase.Passes(FoodSample)"/>.
        /// Returns true when the food sample is in one of the specified sampling periods.
        /// If no periods filters are specified, then all samples should be included. 
        /// </summary>
        /// <param name="foodSample"></param>
        /// <returns></returns>
        public override bool Passes(FoodSample foodSample) {
            if (Periods?.Any() ?? false) {
                if (foodSample.DateSampling == null) {
                    // If sampling date not specified, return default value for missing sampling dates.
                    return IncludeUnspecifiedSamplingDates;
                } else {
                    // Return true if the sampling date matches any of the period definitions.
                    return Periods.Any(r => foodSample.DateSampling >= r.StartDate && foodSample.DateSampling <= r.EndDate);
                }
            }
            // If no periods filters are specified, then all samples should be included
            return true;
        }
    }
}
