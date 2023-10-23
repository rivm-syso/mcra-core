using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed  class HbmCumulativeIndividualDayConcentration : HbmCumulativeIndividualConcentration {

        /// <summary>
        /// The simulated individual day id.
        /// </summary>
        public int SimulatedIndividualDayId { get; set; }

        /// <summary>
        /// The (survey)day of the exposure.
        /// </summary>
        public string Day { get; set; }
    }
}
