using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class SoilBioaccessibilityFraction {
        public Compound Substance { get; set; }
        public double Value { get; set; }
        public BioaccessibilityFractionDistributionType DistributionType { get; set; }
        public double? CvVariability { get; set; }
    }
}
