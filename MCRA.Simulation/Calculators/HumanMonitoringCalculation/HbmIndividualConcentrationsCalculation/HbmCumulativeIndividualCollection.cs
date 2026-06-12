using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualDayConcentrationCalculation;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmIndividualConcentrationCalculation {
    public sealed class HbmCumulativeIndividualCollection : HbmCumulativeIndividualDayCollection {

        /// <summary>
        /// The HBM individual concentrations.
        /// </summary>
        public ICollection<HbmCumulativeIndividualConcentration> HbmCumulativeIndividualConcentrations { get; set; }
    }
}
