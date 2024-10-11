using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class TargetExposureModel : StrongEntity {
        public Compound Compound { get; set; }
        public Dictionary<double, TargetExposurePercentile> TargetExposurePercentiles { get; set; }
        public ExternalExposureUnit ExposureUnit { get; set; } = ExternalExposureUnit.mgPerKgBWPerDay;
    }
}
