using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers.ISampleOriginInfo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Calculators.OccurrencePatternsCalculation {

    public sealed class MarginalOccurrencePatternsCalculator {

        /// <summary>
        /// Computes occurrence patterns info for specified modelled foods and substances
        /// based on the provided agricultural use data and samples.
        /// </summary>
        /// <param name="foods"></param>
        /// <param name="occurrencePatterns"></param>
        /// <param name="samples"></param>
        /// <returns></returns>
        public Dictionary<Food, List<MarginalOccurrencePattern>> ComputeMarginalOccurrencePatterns(
            ICollection<Food> foods,
            ICollection<OccurrencePattern> occurrencePatterns,
            IDictionary<Food, List<ISampleOrigin>> sampleOrigins
        ) {
            var occurrencePatternsPerFood = occurrencePatterns.ToLookup(op => op.Food);
            var result = foods
                .Where(f => occurrencePatternsPerFood.Contains(f))
                .ToDictionary(f => f, f => {
                    var foodOccurrencePatterns = occurrencePatternsPerFood[f].ToList();
                    var foodSampleOrigins = sampleOrigins.ContainsKey(f) ? sampleOrigins[f] : new List<ISampleOrigin>();
                    return calculateFoodMarginalOccurrencePatterns(f, foodOccurrencePatterns, foodSampleOrigins);
                });
            return result;
        }

        /// <summary>
        /// Calculates the marginal occurrence patterns percentages of this food.
        /// </summary>
        /// <param name="occurrencePatterns"></param>
        /// <param name="samples"></param>
        /// <returns></returns>
        private List<MarginalOccurrencePattern> calculateFoodMarginalOccurrencePatterns(
            Food food,
            List<OccurrencePattern> occurrencePatterns,
            List<ISampleOrigin> sampleOrigins
        ) {
            var foodMarginalOccurrencePatterns = new Dictionary<string, MarginalOccurrencePattern>();
            foreach (var sampleLocationFraction in sampleOrigins) {
                var locationOccurrencePatterns = occurrencePatterns.FilterByLocation(sampleLocationFraction.Location);
                foreach (var occurrencePattern in locationOccurrencePatterns) {
                    if (!foodMarginalOccurrencePatterns.ContainsKey(occurrencePattern.Code)) {
                        var marginalOccurrencePatternGroup = new MarginalOccurrencePattern() {
                            Food = food,
                            Code = occurrencePattern.Code,
                            Compounds = occurrencePattern.Compounds.ToHashSet(),
                            OccurrenceFraction = 0
                        };
                        foodMarginalOccurrencePatterns.Add(occurrencePattern.Code, marginalOccurrencePatternGroup);
                    }
                    foodMarginalOccurrencePatterns[occurrencePattern.Code].OccurrenceFraction += sampleLocationFraction.Fraction * (occurrencePattern.OccurrenceFraction);
                    foodMarginalOccurrencePatterns[occurrencePattern.Code].OccurrenceFraction
                        = Math.Min(foodMarginalOccurrencePatterns[occurrencePattern.Code].OccurrenceFraction, 1D);
                }
            }
            return foodMarginalOccurrencePatterns.Values.ToList();
        }
    }
}
