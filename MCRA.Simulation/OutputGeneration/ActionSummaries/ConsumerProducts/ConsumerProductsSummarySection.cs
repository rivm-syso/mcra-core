using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductsSummarySection : SummarySection {

        public List<ConsumerProductsSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<ConsumerProduct> consumerProducts) {
            Records = [.. consumerProducts
                .Select(c => {
                    return new ConsumerProductsSummaryRecord() {
                        Code = c.Code,
                        Name = c.Name,
                        CodeParent = c.Parent?.Code,
                        Description = c.Description,
                    };
                })
                .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)];
        }
    }
}
