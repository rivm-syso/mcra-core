using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes bod indicator conversion data.
    /// </summary>
    public sealed class BodIndicatorConversionsSummarySection : SummarySection {

        public List<BodIndicatorConversionSummaryRecord> Records { get; set; }

        public void Summarize(List<BodIndicatorConversion> bodIndicatorConversions) {
            Records = [.. bodIndicatorConversions
                .Select(r => new BodIndicatorConversionSummaryRecord {
                    FromIndicator = r.FromIndicator.GetShortDisplayName(),
                    FromUnit = r.FromUnit,
                    ToIndicator = r.ToIndicator.GetShortDisplayName(),
                    ToUnit = r.ToUnit,
                    Value = r.Value
                }
            )];
        }
    }
}
