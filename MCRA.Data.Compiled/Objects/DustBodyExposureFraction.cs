using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class DustBodyExposureFraction {
        public string idSubgroup { get; set; }
        public double? AgeLower { get; set; }
        public GenderType Sex { get; set; }
        public double Value { get; set; }
        public ProbabilityDistribution DistributionType { get; set; }
        public double? CvVariability { get; set; }
    }
}
