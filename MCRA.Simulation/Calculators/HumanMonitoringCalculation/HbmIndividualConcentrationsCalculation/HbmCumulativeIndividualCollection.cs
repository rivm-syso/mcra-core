using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation {
    public sealed class HbmCumulativeIndividualCollection {

        public TargetUnit TargetUnit { get; set; }

        public ICollection<HbmCumulativeIndividualConcentration> HbmCumulativeIndividualConcentrations { get; set; }

    }
}
