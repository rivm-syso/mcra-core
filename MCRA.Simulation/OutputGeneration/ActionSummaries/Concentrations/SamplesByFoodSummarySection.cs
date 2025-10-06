using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes the samples per food.
    /// </summary>
    public sealed class SamplesByFoodSummarySection : SummarySection {

        public List<SamplesByFoodSummaryRecord> Records { get; set; }

        public void Summarize(ILookup<Food, FoodSample> foodSamples) {
            Records = foodSamples
                .Select(r => new SamplesByFoodSummaryRecord() {
                    FoodCode = r.Key.Code,
                    FoodName = r.Key.Name,
                    NumberOfSamples = r.Count()
                })
                .OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}

