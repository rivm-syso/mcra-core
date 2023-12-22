using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Filters.FoodSampleFilters;

namespace MCRA.Simulation.OutputGeneration
{
    public sealed class ConcentrationLimitExceedancesByFoodDataSection : ActionSummarySectionBase {

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
                    var filteredFoodSamplesCount = foodSamples.Count(r => !filter.Passes(r));
                    var result = new ConcentrationLimitExceedanceByFoodDataRecord() {
                        FoodCode = food.Code,
                        FoodName = food.Name,
                        TotalNumberOfSamples = foodSamples.Count,
                        NumberOfSamplesExceedingLimit = filteredFoodSamplesCount,
                        FractionOfTotal = (double)filteredFoodSamplesCount / foodSamples.Count
                    };
                    return result;
                })
                .Where(r => !double.IsNaN(r.FractionOfTotal) && r.FractionOfTotal > 0)
                .OrderBy(c => c.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
