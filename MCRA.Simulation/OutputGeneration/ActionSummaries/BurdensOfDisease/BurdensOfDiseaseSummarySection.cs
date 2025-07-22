using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes burden of disease data.
    /// </summary>
    public sealed class BurdensOfDiseaseSummarySection : SummarySection {

        public List<BurdensOfDiseaseSummaryRecord> Records { get; set; }

        public void Summarize(List<BurdenOfDisease> burdensOfDisease) {
            Records = burdensOfDisease
                .Select(r => {
                    var record = new BurdensOfDiseaseSummaryRecord {
                        Population = r.Population.Code,
                        Effect = r.Effect.Name,
                        BodIndicator = r.BodIndicator.GetDisplayName(),
                        Value = r.Value
                    };
                    return record;
                })
                .ToList();
        }
    }
}
