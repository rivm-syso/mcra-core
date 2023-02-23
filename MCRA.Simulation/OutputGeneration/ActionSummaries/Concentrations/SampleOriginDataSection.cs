using MCRA.Data.Compiled.Wrappers.ISampleOriginInfo;
using System.Collections.Generic;
using System.Linq;

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
                    Fraction = r.Fraction,
                    NumberOfSamples = r.NumberOfSamples,
                    Origin = !r.IsUndefinedLocation ? r.Location : "Unknown"
                })
                .OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.Origin, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.Origin == "Unknown")
                .ToList();
        }
    }
}

