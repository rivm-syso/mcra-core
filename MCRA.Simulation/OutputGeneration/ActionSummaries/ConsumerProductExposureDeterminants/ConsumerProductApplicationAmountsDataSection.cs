using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductApplicationAmountsDataSection : SummarySection {
        public List<ConsumerProductApplicationAmountRecord> Records { get; set; }

        public void Summarize(
            IList<ConsumerProductApplicationAmount> consumerProductApplicationAmounts
        ) {
            Records = [.. consumerProductApplicationAmounts
                .Select(c => {
                    return new ConsumerProductApplicationAmountRecord() {
                        ProductCode = c.Product.Code,
                        ProductName = c.Product.Name,
                        AgeLower = c.AgeLower.HasValue ? c.AgeLower.Value : null,
                        Sex = c.Sex != GenderType.Undefined ? c.Sex.ToString() : null,
                        Amount = c.Amount,
                        DistributionType = c.DistributionType != ApplicationAmountDistributionType.Constant ? c.DistributionType.ToString() : null,
                        CvVariability = c.CvVariability.HasValue ? c.CvVariability.Value : null
                    };
                })];
        }
    }
}