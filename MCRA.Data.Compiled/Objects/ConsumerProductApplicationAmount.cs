using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConsumerProductApplicationAmount {
        public ConsumerProduct Product { get; set; }
        public double Amount { get; set; }
        public ApplicationAmountDistributionType DistributionType { get; set; }
        public double? CvVariability {  get; set; }
    }
}
