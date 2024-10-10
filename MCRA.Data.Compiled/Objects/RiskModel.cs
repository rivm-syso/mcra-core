using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class RiskModel : StrongEntity {
        public Compound Compound { get; set; }

        public Dictionary<double, RiskPercentile> RiskPercentiles { get; set; }

        public string ExposureUnitString { get; set; }

        public ExternalExposureUnit ExposureUnit {
            get {
                if (!string.IsNullOrEmpty(ExposureUnitString)) {
                    return ExternalExposureUnitConverter.FromString(ExposureUnitString);
                }
                return ExternalExposureUnit.mgPerKgBWPerDay;
            }
        }

        public string RiskMetricTypeString { get; set; }
        public RiskMetricType RiskMetric {
            get {
                return RiskMetricTypeConverter.FromString(RiskMetricTypeString, RiskMetricType.HazardExposureRatio);
            }
            set {
                RiskMetricTypeString = value.ToString();
            }
        }
    }
}
