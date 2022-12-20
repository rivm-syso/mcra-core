using System.Collections.Generic;
using System.Linq;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers.ISampleOriginInfo;

namespace MCRA.Simulation.Calculators.SampleOriginCalculation {
    public sealed class SampleOriginCalculator {

        /// <summary>
        /// Calculates the sample origins and origin fractions for the given set of samples.
        /// </summary>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static IDictionary<Food, List<ISampleOrigin>> Calculate(ILookup<Food, FoodSample> samples) {
            return samples.ToDictionary(g => g.Key, g => Calculate(g.Key, g));
        }

        /// <summary>
        /// Calculates the sample origins and origin fractions for the given set of samples.
        /// </summary>
        /// <param name="samples"></param>
        /// <returns></returns>
        public static List<ISampleOrigin> Calculate(Food food, IEnumerable<FoodSample> samples) {
            // Grouped the samples by location and compute the origin fractions
            var totalSampleCount = 0;
            var locationCountsDict = new Dictionary<string, int>();

            foreach (var s in samples) {
                totalSampleCount++;
                var key = s.Location ?? string.Empty;
                _ = locationCountsDict.TryGetValue(key, out int count);
                locationCountsDict[key] = count + 1;
            }

            var sampleOrigins = locationCountsDict
                .Select(spl => new SampleOriginRecord() {
                    Food = food,
                    Location = spl.Key,
                    Fraction = (float)spl.Value / totalSampleCount,
                    NumberOfSamples = spl.Value,
                })
                .Cast<ISampleOrigin>()
                .ToList();

            // If the "undefined" sample location does not exist, add it as zero
            if (!sampleOrigins.Any(spl => spl.IsUndefinedLocation)) {
                sampleOrigins.Add(new SampleOriginRecord() {
                    Food = food,
                    Location = string.Empty,
                    Fraction = 1 - sampleOrigins.Sum(spl => spl.Fraction),
                    NumberOfSamples = 0,
                });
            }

            return sampleOrigins;
        }
    }
}
