using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class RiskModel : StrongEntity {
        public Compound Compound { get; set; }
        public Dictionary<double, RiskPercentile> RiskPercentiles { get; set; }
        public ExternalExposureUnit ExposureUnit { get; set; } = ExternalExposureUnit.mgPerKgBWPerDay;
        public RiskMetricType RiskMetric { get; set; } = RiskMetricType.HazardExposureRatio;
        public double ThresholdMarginOfExposure { get; set; } = 100D;
        public double LeftMarginSafetyPlot { get; set; } = 0.1D;
        public double RightMarginSafetyPlot { get; set; } = 10000000D;
        public double ConfidenceInterval { get; set; } = 90D;
    }
}
