using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureTargetPair {

        public ExposureTargetPair() { }

        public ExposureTargetPair(ExposureTarget targetFrom, ExposureTarget targetTo) {
            TargetFrom = targetFrom;
            TargetTo = targetTo;
        }

        public ExposureTarget TargetFrom { get; set; }

        public ExposureTarget TargetTo { get; set; }

        public override bool Equals(object obj) {
            return obj is ExposureTargetPair other &&
            TargetFrom == other.TargetFrom &&
            TargetTo == other.TargetTo;
        }

        public override int GetHashCode() {
            return HashCode.Combine(TargetFrom, TargetTo);
        }
    }
}
