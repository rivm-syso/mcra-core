using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation {
    public sealed class HbmIndividualDayCollection {

        public TargetUnit TargetUnit { get; set; }

        public ICollection<HbmIndividualDayConcentration> HbmIndividualDayConcentrations { get; set; }

    }
}
