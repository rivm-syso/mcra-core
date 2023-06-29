using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes the sample origin fractions per food.
    /// </summary>
    public sealed class SamplePropertyDataSection : SummarySection {

        public string PropertyName { get; set; }

        public List<SamplePropertyDataRecord> Records { get; set; }

        public void Summarize(ILookup<Food, FoodSample> foodSamples, string propertyName, Func<FoodSample, string> propertyValueExtractor) {
            PropertyName = propertyName;
            Records = foodSamples
                .SelectMany(s => {
                    var totalFoodSamples = s.Count();
                    var records = s.GroupBy(r => propertyValueExtractor(r))
                        .Select(r => new SamplePropertyDataRecord() {
                            FoodCode = s.Key.Code,
                            FoodName = s.Key.Name,
                            PropertyValue = r.Key,
                            NumberOfSamples = r.Count(),
                            Percentage = (double)r.Count() / totalFoodSamples * 100,
                        })
                        .OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                        .ThenBy(r => r.PropertyValue, StringComparer.OrdinalIgnoreCase)
                        .ToList();
                    return records;
                })
                .ToList();
        }
    }
}

