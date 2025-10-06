using MCRA.Simulation.Objects;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes the sample origin fractions per food.
    /// </summary>
    public sealed class SampleOriginsSummarySection : SummarySection {

        public List<SampleOriginSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<ISampleOrigin> sampleOriginInfos) {
            Records = sampleOriginInfos
                .Where(r => r.Fraction > 0)
                .Select(r => new SampleOriginSummaryRecord() {
                    FoodCode = r.Food.Code,
                    FoodName = r.Food.Name,
                    Percentage = r.Fraction * 100,
                    NumberOfSamples = r.NumberOfSamples,
                    Origin = !r.IsUndefinedLocation ? r.Location : "Unknown"
                })
                .OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.Origin, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.Origin == "Unknown")
                .ToList();
        }
    }
}

