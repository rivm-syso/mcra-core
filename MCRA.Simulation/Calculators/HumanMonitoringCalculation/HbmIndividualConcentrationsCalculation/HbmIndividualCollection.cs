using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation {
    public sealed class HbmIndividualCollection {
        public TargetUnit TargetUnit { get; set; }

        /// <summary>
        /// The exposure target of the collection.
        /// </summary>
        public ExposureTarget Target {
            get {
                return TargetUnit.Target;
            }
        }
        public ICollection<HbmIndividualConcentration> HbmIndividualConcentrations { get; set; }

    }
}
