using MCRA.Utils.ExtensionMethods;
using System;

namespace MCRA.Data.Compiled.Objects {
    public sealed class HumanMonitoringSamplingMethod {

        public string ExposureRoute { get; set; }

        public string BiologicalMatrixCode { get; set; }

        public string SampleTypeCode { get; set; }

        public bool IsExternal {
            get {
                return !string.IsNullOrEmpty(ExposureRoute);
            }
        }

        public string Code {
            get {
                if (!string.IsNullOrEmpty(BiologicalMatrixCode)) {
                    return $"{BiologicalMatrixCode}_{SampleTypeCode}";
                } else {
                    return $"{ExposureRoute}_{SampleTypeCode}";
                }
            }
        }

        public string Name {
            get {
                if (!string.IsNullOrEmpty(BiologicalMatrixCode)) {
                    return $"{BiologicalMatrixCode} ({SampleTypeCode})";
                } else {
                    return $"{ExposureRoute} ({SampleTypeCode})";
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
                BiologicalMatrixCode = this.BiologicalMatrixCode,
                ExposureRoute = this.ExposureRoute,
                SampleTypeCode = this.SampleTypeCode,
            };
        }

        public override int GetHashCode() {
            return Code.GetChecksum();
        }
    }
}
