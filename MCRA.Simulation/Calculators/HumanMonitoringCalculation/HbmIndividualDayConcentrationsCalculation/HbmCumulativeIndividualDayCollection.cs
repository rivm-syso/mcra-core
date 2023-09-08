using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation {
    public sealed class HbmCumulativeIndividualDayCollection {

        public TargetUnit TargetUnit { get; set; }

        public ICollection<HbmCumulativeIndividualDayConcentration> HbmCumulativeIndividualDayConcentrations { get; set; }

    }
}
