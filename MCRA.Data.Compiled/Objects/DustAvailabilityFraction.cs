using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class DustAvailabilityFraction {
        public string idSubgroup { get; set; }
        public Compound Substance { get; set; }
        public double? AgeLower { get; set; }
        public GenderType Sex { get; set; }
        public double Value { get; set; }
        public DustAvailabilityFractionDistributionType DistributionType { get; set; }
        public double? CvVariability { get; set; }
    }
}
