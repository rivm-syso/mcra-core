using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Data.Compiled.Objects {
    public sealed class HumanMonitoringSamplingMethod {

        public string ExposureRoute { get; set; }

        public BiologicalMatrix BiologicalMatrix { get; set; }

        public string SampleTypeCode { get; set; }

        public bool IsExternal {
            get {
                return !string.IsNullOrEmpty(ExposureRoute);
            }
        }

        public string Code {
            get {
                if (BiologicalMatrix != BiologicalMatrix.Undefined) {
                    return $"{BiologicalMatrix}_{SampleTypeCode}";
                } else {
                    return $"{ExposureRoute}_{SampleTypeCode}";
                }
            }
        }

        public string Name {
            get {
                var result = (BiologicalMatrix != BiologicalMatrix.Undefined)
                    ? BiologicalMatrix.GetDisplayName()
                    : ExposureRoute;
                if (!string.IsNullOrEmpty(SampleTypeCode)) {
                    result += " - " + SampleTypeCode;
                }
                return result;
            }
        }

        private static readonly HashSet<BiologicalMatrix> _bloodMatrices = new HashSet<BiologicalMatrix>() {
            BiologicalMatrix.Blood,
            BiologicalMatrix.BloodPlasma,
            BiologicalMatrix.BloodSerum,
            BiologicalMatrix.CordBlood,
            BiologicalMatrix.VenousBlood,
            BiologicalMatrix.ArterialBlood,
            BiologicalMatrix.BrainBlood,
        };

        private static readonly HashSet<BiologicalMatrix> _urineMatrices = new HashSet<BiologicalMatrix>() {
            BiologicalMatrix.Urine
        };

        public bool IsBlood { get { return _bloodMatrices.Contains(BiologicalMatrix); } }

        public bool IsUrine { get { return _urineMatrices.Contains(BiologicalMatrix); } }

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
