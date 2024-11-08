using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Filters.FoodSampleFilters {
    public sealed class SamplePropertyFilter : FoodSampleFilterBase {

        /// <summary>
        /// The locations from which the samples should be included.
        /// </summary>
        private readonly HashSet<string> _keyWords;

        /// <summary>
        /// Specifies whether samples with unspecified sampling locations should be included or not.
        /// </summary>
        private readonly bool _includeUnspecifiedSampleLocations;

        /// <summary>
        /// The sample property value extractor for the filter.
        /// </summary>
        private readonly Func<FoodSample, string> _propertyValueExtractor;

        /// <summary>
        /// Initializes a new <see cref="FoodSampleFilterBase"/> instance.
        /// </summary>
        /// <param name="keyWords"></param>
        /// <param name="propertyValueExtractor"></param>
        /// <param name="includeUnspecifiedSampleLocations"></param>
        public SamplePropertyFilter(
            ICollection<string> keyWords,
            Func<FoodSample, string> propertyValueExtractor,
            bool includeUnspecifiedSampleLocations
        ) : base() {
            _keyWords = keyWords.ToHashSet(StringComparer.OrdinalIgnoreCase);
            _propertyValueExtractor = propertyValueExtractor;
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
            if (_keyWords?.Count > 0) {
                var value = _propertyValueExtractor(foodSample);
                if (string.IsNullOrEmpty(value)) {
                    // If sample location is not specified, return default value for missing sampling dates.
                    return _includeUnspecifiedSampleLocations;
                } else {
                    // Return true if the sampling location matches any of the specified locations.
                    return _keyWords.Contains(value);
                }
            }
            // If no location filters are specified, then all samples should be included
            return true;
        }
    }
}
