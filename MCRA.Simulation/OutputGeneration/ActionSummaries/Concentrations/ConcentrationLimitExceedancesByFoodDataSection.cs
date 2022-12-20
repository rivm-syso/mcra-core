using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Filters.FoodSampleFilters;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConcentrationLimitExceedancesByFoodDataSection : ActionSummaryBase {

        public double ExceedanceFactionThreshold { get; set; }

        public List<ConcentrationLimitExceedanceByFoodDataRecord> Records { get; set; }

        public void Summarize(
            ICollection<ConcentrationLimit> limits,
            ILookup<Food, FoodSample> foodSamplesLookup,
            double exceedanceFactionThreshold
        ) {
            var filter = new MrlExceedanceSamplesFilter(limits, exceedanceFactionThreshold);
            ExceedanceFactionThreshold = exceedanceFactionThreshold;
            Records = limits
                .Where(r => !double.IsNaN(r.Limit))
                .GroupBy(r => r.Food)
                .AsParallel()
                .Select(g => {
                    var food = g.Key;
                    var foodSamples = foodSamplesLookup.Contains(food) ? foodSamplesLookup[food].ToList() : new List<FoodSample>();
                    var filteredFoodSamplesCount = foodSamples.Where(r => !filter.Passes(r)).Count();
                    var result = new ConcentrationLimitExceedanceByFoodDataRecord() {
                        FoodCode = food.Code,
                        FoodName = food.Name,
                        TotalNumberOfSamples = foodSamplesLookup.Count,
                        NumberOfSamplesExceedingLimit = filteredFoodSamplesCount,
                        FractionOfTotal = (double)filteredFoodSamplesCount / foodSamplesLookup.Count
                    };
                    return result;
                })
                .Where(r => !double.IsNaN(r.FractionOfTotal) && r.FractionOfTotal > 0)
                .OrderBy(c => c.FoodName, System.StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
