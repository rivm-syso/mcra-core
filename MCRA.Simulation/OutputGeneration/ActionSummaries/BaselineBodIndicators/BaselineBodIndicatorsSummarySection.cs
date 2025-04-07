using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes the burden of disease infos.
    /// </summary>
    public sealed class BaselineBodIndicatorsSummarySection : SummarySection {

        public List<BaselineBodIndicatorsSummaryRecord> Records { get; set; }

        public void Summarize(List<BaselineBodIndicator> baselineBodIndicators) {
            Records = baselineBodIndicators
                .Select(r => {
                    var record = new BaselineBodIndicatorsSummaryRecord {
                        Population = r.Population.Code,
                        Effect = r.Effect.Name,
                        BodIndicator = r.BodIndicator.GetDisplayName(),
                        Value = r.Value.ToString()
                    };
                    return record;
                })
                .ToList();
        }
    }
}
