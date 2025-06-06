using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ConsumerProductConcentration {
        public ConsumerProduct Product { get; set; }
        public Compound Substance { get; set; }
        public double Concentration { get; set; }
        public ConcentrationUnit Unit { get; set; } = ConcentrationUnit.ugPerKg;
        public ConsumerProductConcentrationDistributionType DistributionType { get; set; }
        public double? CvVariability { get; set; }
        public double? OccurrencePercentage { get; set; }
    }
}
