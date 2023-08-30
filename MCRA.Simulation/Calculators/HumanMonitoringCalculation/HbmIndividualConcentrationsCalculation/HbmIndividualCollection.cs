using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation {
    public sealed class HbmIndividualCollection {

        public TargetUnit TargetUnit { get; set; }

        public ICollection<HbmIndividualConcentration> HbmIndividualConcentrations { get; set; }

    }
}
