using MCRA.Data.Compiled.Interfaces;
using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class DustConcentrationDistribution : IConcentrationDistribution {
        public string IdDistribution { get; set; }
        public Compound Substance { get; set; }
        public double Mean { get; set; }
        public ConcentrationUnit Unit { get; set; } = ConcentrationUnit.ugPerKg;
        public DustConcentrationDistributionType DistributionType { get; set; }
        public double? CvVariability { get; set; }
        public double? OccurrencePercentage { get; set; }
    }
}
