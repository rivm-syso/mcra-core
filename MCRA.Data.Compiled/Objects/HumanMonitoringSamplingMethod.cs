using MCRA.Utils.ExtensionMethods;
using System;

namespace MCRA.Data.Compiled.Objects {
    public sealed class HumanMonitoringSamplingMethod {

        public string Compartment { get; set; }

        public string ExposureRoute { get; set; }

        public string SampleType { get; set; }

        public bool IsExternal {
            get {
                return !string.IsNullOrEmpty(ExposureRoute);
            }
        }

        public string Code {
            get {
                if (!string.IsNullOrEmpty(Compartment)) {
                    return $"{Compartment}_{SampleType}";
                } else {
                    return $"{ExposureRoute}_{SampleType}";
                }
            }
        }

        public string Name {
            get {
                if (!string.IsNullOrEmpty(Compartment)) {
                    return $"{Compartment} ({SampleType})";
                } else {
                    return $"{ExposureRoute} ({SampleType})";
                }
            }
        }

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            var t = (HumanMonitoringSamplingMethod)obj;
            return string.Equals(this.Code, t.Code, StringComparison.OrdinalIgnoreCase);
        }

        public HumanMonitoringSamplingMethod Clone() {
            return new HumanMonitoringSamplingMethod() {
                Compartment = this.Compartment,
                ExposureRoute = this.ExposureRoute,
                SampleType = this.SampleType,
            };
        }

        public override int GetHashCode() {
            return Code.GetChecksum();
        }
    }
}
