using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class RiskModel : IStrongEntity {

        private string _name;

        public string Code { get; set; }

        public string Name {
            get {
                if (!string.IsNullOrEmpty(_name)) {
                    return _name;
                }
                return Code;
            }
            set {
                _name = value;
            }
        }

        public string Description { get; set; }

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
