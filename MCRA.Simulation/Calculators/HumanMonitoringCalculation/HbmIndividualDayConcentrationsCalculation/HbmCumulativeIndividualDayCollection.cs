using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation {
    public sealed class HbmCumulativeIndividualDayCollection {

        /// <summary>
        /// The target and unit.
        /// </summary>
        public TargetUnit TargetUnit { get; set; }

        /// <summary>
        /// The HBM individual concentrations.
        /// </summary>
        public ICollection<HbmCumulativeIndividualDayConcentration> HbmCumulativeIndividualDayConcentrations { get; set; }

    }
}
