using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ProcessingTypesSummarySection : SummarySection {

        public List<ProcessingTypeSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<ProcessingType> processingTypes) {
            Records = processingTypes
                .Select(c => new ProcessingTypeSummaryRecord() {
                    Name = c.Name,
                    Code = c.Code,
                    Bulking = c.IsBulkingBlending ? "yes" : "no",
                    Distribution = c.DistributionType.GetDisplayName(),
                })
                .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
