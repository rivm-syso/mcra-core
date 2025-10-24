using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.Calculators.OccupationalExposureCalculation {
    public sealed class OccupationalTaskDeterminants {

        public RPEType RPEType { get; set; }

        public static bool operator ==(OccupationalTaskDeterminants val1, OccupationalTaskDeterminants val2) {
            if (ReferenceEquals(val1, val2)) {
                return true;
            } else if (val1 is null || val2 is null) {
                return false;
            } else {
                return val1.RPEType == val2.RPEType;
            }
        }

        public static bool operator !=(OccupationalTaskDeterminants val1, OccupationalTaskDeterminants val2) {
            return !(val1 == val2);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = RPEType.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString() {
            return RPEType.GetDisplayName();
        }

        public override bool Equals(object obj) {
            if (obj is OccupationalTaskDeterminants val) {
                return this == val;
            }
            return base.Equals(obj);
        }
    }
}


