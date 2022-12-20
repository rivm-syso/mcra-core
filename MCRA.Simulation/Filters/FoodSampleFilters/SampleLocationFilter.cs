using MCRA.Data.Compiled.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Filters.FoodSampleFilters {
    public sealed class SampleLocationFilter : FoodSampleFilterBase {

        /// <summary>
        /// The locations from which the samples should be included.
        /// </summary>
        public HashSet<string> Locations { get; set; }

        /// <summary>
        /// Specifies whether samples with unspecified sampling locations should be included or not.
        /// </summary>
        public bool IncludeUnspecifiedSampleLocations { get; set; } = true;

        /// <summary>
        /// Initializes a new <see cref="FoodSampleFilterBase"/> instance.
        /// </summary>
        /// <param name="locations"></param>
        /// <param name="includeUnspecifiedSampleLocations"></param>
        public SampleLocationFilter(List<string> locations, bool includeUnspecifiedSampleLocations) : base() {
            Locations = locations.ToHashSet(StringComparer.OrdinalIgnoreCase);
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
            if (Locations?.Any() ?? false) {
                if (foodSample.Location == null) {
                    // If sample location is not specified, return default value for missing sampling dates.
                    return IncludeUnspecifiedSampleLocations;
                } else {
                    // Return true if the sampling location matches any of the specified locations.
                    return Locations.Contains(foodSample.Location);
                }
            }
            // If no location filters are specified, then all samples should be included
            return true;
        }
    }
}
