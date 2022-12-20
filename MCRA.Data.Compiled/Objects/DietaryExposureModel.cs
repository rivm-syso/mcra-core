using MCRA.General;
using System.Collections.Generic;

namespace MCRA.Data.Compiled.Objects {
    public sealed class DietaryExposureModel : IStrongEntity {

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

        public Dictionary<double, DietaryExposurePercentile> DietaryExposurePercentiles { get; set; }

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
