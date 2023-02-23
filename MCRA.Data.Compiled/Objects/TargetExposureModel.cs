using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class TargetExposureModel : IStrongEntity {

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

        public string ExposureUnitString { get; set; }

        public Dictionary<double, TargetExposurePercentile> TargetExposurePercentiles { get; set; }

        public ExposureUnit ExposureUnit {
            get {
                if (!string.IsNullOrEmpty(ExposureUnitString)) {
                    return ExposureUnitConverter.FromString(ExposureUnitString);
                }
                return ExposureUnit.mgPerKgBWPerDay;
            }
        }
    }
}
