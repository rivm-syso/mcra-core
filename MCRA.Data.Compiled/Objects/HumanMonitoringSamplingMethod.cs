using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Data.Compiled.Objects {
    public sealed class HumanMonitoringSamplingMethod {

        public string ExposureRoute { get; set; }

        public BiologicalMatrix BiologicalMatrix { get; set; }

        public string SampleTypeCode { get; set; }

        public string Code {
            get {
                if (!BiologicalMatrix.IsUndefined()) {
                    return $"{BiologicalMatrix}_{SampleTypeCode}";
                } else {
                    return $"{ExposureRoute}_{SampleTypeCode}";
                }
            }
        }

        public string Name {
            get {
                var name = (!BiologicalMatrix.IsUndefined()) ? BiologicalMatrix.GetDisplayName() : ExposureRoute;
                if (!string.IsNullOrEmpty(SampleTypeCode) && !name.Contains(SampleTypeCode, StringComparison.InvariantCultureIgnoreCase)) {
                    name += " " + SampleTypeCode.ToLower();
                }
                return name;
            }
        }

        public bool IsBlood => BiologicalMatrix.IsBlood();

        public bool IsUrine => BiologicalMatrix.IsUrine();

        public override bool Equals(object obj) {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }
            var t = (HumanMonitoringSamplingMethod)obj;
            return string.Equals(this.Code, t.Code, StringComparison.OrdinalIgnoreCase);
        }

        public HumanMonitoringSamplingMethod Clone() {
            return new HumanMonitoringSamplingMethod() {
                BiologicalMatrix = this.BiologicalMatrix,
                ExposureRoute = this.ExposureRoute,
                SampleTypeCode = this.SampleTypeCode,
            };
        }

        public override int GetHashCode() {
            return Code.GetChecksum();
        }
    }
}
