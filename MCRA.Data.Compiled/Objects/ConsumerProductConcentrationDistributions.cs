using MCRA.Data.Compiled.Interfaces;
using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConsumerProductConcentrationDistribution : IConcentrationDistribution {
        public string IdDistribution => Product?.ToString();
        public ConsumerProduct Product { get; set; }
        public Compound Substance { get; set; }
        public double Mean { get; set; }
        public ConsumerProductConcentrationUnit Unit { get; set; }
        public ConsumerProductConcentrationDistributionType DistributionType { get; set; }
        public double? CvVariability { get; set; }
        public double? OccurrencePercentage { get; set; }
    }
}
