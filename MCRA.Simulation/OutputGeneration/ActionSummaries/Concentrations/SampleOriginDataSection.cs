using MCRA.Data.Compiled.Wrappers.ISampleOriginInfo;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes the sample origin fractions per food.
    /// </summary>
    public sealed class SampleOriginDataSection : SummarySection {

        public List<SampleOriginDataRecord> SampleOriginDataRecords { get; set; }

        public void Summarize(ICollection<ISampleOrigin> sampleOriginInfos) {
            SampleOriginDataRecords = sampleOriginInfos
                .Where(r => r.Fraction > 0)
                .Select(r => new SampleOriginDataRecord() {
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

