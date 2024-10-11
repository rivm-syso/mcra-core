using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class RiskModel : StrongEntity {
        public Compound Compound { get; set; }
        public Dictionary<double, RiskPercentile> RiskPercentiles { get; set; }
        public ExternalExposureUnit ExposureUnit { get; set; } = ExternalExposureUnit.mgPerKgBWPerDay;
        public RiskMetricType RiskMetric { get; set; } = RiskMetricType.HazardExposureRatio;
    }
}
