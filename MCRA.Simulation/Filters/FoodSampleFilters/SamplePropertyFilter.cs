using MCRA.Data.Compiled.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Filters.FoodSampleFilters {
    public sealed class SamplePropertyFilter : FoodSampleFilterBase {

        /// <summary>
        /// The locations from which the samples should be included.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The locations from which the samples should be included.
        /// </summary>
        public HashSet<string> KeyWords { get; set; }

        /// <summary>
        /// Specifies whether samples with unspecified sampling locations should be included or not.
        /// </summary>
        public bool IncludeUnspecifiedSampleLocations { get; set; } = true;

        /// <summary>
        /// The sample property value extractor for the filter.
        /// </summary>
        public Func<FoodSample, string> PropertyValueExtractor { get; set; }

        /// <summary>
        /// Initializes a new <see cref="FoodSampleFilterBase"/> instance.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="keyWords"></param>
        /// <param name="propertyValueExtractor"></param>
        /// <param name="includeUnspecifiedSampleLocations"></param>
        public SamplePropertyFilter(
            string propertyName,
            ICollection<string> keyWords,
            Func<FoodSample, string> propertyValueExtractor,
            bool includeUnspecifiedSampleLocations
        ) : base() {
            PropertyName = propertyName;
            KeyWords = keyWords.ToHashSet(StringComparer.OrdinalIgnoreCase);
            PropertyValueExtractor = propertyValueExtractor;
            IncludeUnspecifiedSampleLocations = includeUnspecifiedSampleLocations;
        }

        /// <summary>
        /// Implements <see cref="FoodSampleFilterBase.Passes(FoodSample)"/>.
        /// Returns true when the sample is in one of the specified sampling periods.
        /// If no periods filters are specified, then all samples should be included. 
        /// </summary>
        /// <param name="foodSample"></param>
        /// <returns></returns>
        public override bool Passes(FoodSample foodSample) {
            if (KeyWords?.Any() ?? false) {
                var value = PropertyValueExtractor(foodSample);
                if (string.IsNullOrEmpty(value)) {
                    // If sample location is not specified, return default value for missing sampling dates.
                    return IncludeUnspecifiedSampleLocations;
                } else {
                    // Return true if the sampling location matches any of the specified locations.
                    return KeyWords.Contains(value);
                }
            }
            // If no location filters are specified, then all samples should be included
            return true;
        }
    }
}
