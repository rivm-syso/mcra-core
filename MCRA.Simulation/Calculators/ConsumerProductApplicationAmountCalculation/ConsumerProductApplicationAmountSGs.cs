using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.ConsumerProductApplicationAmountCalculation {
    public sealed class ConsumerProductApplicationAmountSGs {
        public ConsumerProduct Product { get; set; }
        public double? Amount { get; set; }
        public ApplicationAmountUnit AmountUnit { get; set; }
        public ApplicationAmountDistributionType DistributionType { get; set; }
        public double? CvVariability { get; set; }
        public ICollection<ConsumerProductApplicationAmount> CPAASubgroups { get; set; }
    }
}
