using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class DustAdherenceAmount {
        public string idSubgroup { get; set; }
        public double? AgeLower { get; set; }
        public GenderType Sex { get; set; }
        public double Value { get; set; }
        public DustAdherenceAmountDistributionType DistributionType { get; set; }
        public double? CvVariability { get; set; }
    }
}
