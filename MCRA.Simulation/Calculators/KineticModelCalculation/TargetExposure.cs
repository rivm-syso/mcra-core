using MCRA.General;

namespace MCRA.Simulation.Calculators.KineticModelCalculation {
    public class TargetExposure {

        /// <summary>
        /// The target unit.
        /// </summary>
        public TargetUnit TargetUnit { get; set; }

        /// <summary>
        /// The exposure target of the collection.
        /// </summary>
        public ExposureTarget Target {
            get {
                return TargetUnit.Target;
            }
        }

        public string Compartment { get; set; }

        public double RelativeCompartmentWeight { get; set; } = double.NaN;
    }
}