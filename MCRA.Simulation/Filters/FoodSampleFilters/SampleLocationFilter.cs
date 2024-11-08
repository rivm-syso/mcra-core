using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Filters.FoodSampleFilters {
    public sealed class SampleLocationFilter : FoodSampleFilterBase {

        /// <summary>
        /// The locations from which the samples should be included.
        /// </summary>
        private readonly HashSet<string> _locations;

        /// <summary>
        /// Specifies whether samples with unspecified sampling locations should be included or not.
        /// </summary>
        private readonly bool _includeUnspecifiedSampleLocations;

        /// <summary>
        /// Initializes a new <see cref="FoodSampleFilterBase"/> instance.
        /// </summary>
        /// <param name="locations"></param>
        /// <param name="includeUnspecifiedSampleLocations"></param>
        public SampleLocationFilter(List<string> locations, bool includeUnspecifiedSampleLocations) : base() {
            _locations = locations.ToHashSet(StringComparer.OrdinalIgnoreCase);
            _includeUnspecifiedSampleLocations = includeUnspecifiedSampleLocations;
        }

        /// <summary>
        /// Implements <see cref="FoodSampleFilterBase.Passes(FoodSample)"/>.
        /// Returns true when the sample is in one of the specified sampling periods.
        /// If no periods filters are specified, then all samples should be included.
        /// </summary>
        /// <param name="foodSample"></param>
        /// <returns></returns>
        public override bool Passes(FoodSample foodSample) {
            if (_locations?.Count > 0) {
                if (foodSample.Location == null) {
                    // If sample location is not specified, return default value for missing sampling dates.
                    return _includeUnspecifiedSampleLocations;
                } else {
                    // Return true if the sampling location matches any of the specified locations.
                    return _locations.Contains(foodSample.Location);
                }
            }
            // If no location filters are specified, then all samples should be included
            return true;
        }
    }
}
