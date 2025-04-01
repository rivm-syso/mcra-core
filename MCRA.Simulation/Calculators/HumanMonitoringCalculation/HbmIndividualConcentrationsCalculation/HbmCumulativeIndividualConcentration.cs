using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public class HbmCumulativeIndividualConcentration {

        /// <summary>
        /// The original individual entity.
        /// </summary>
        public SimulatedIndividual SimulatedIndividual { get; set; }

        /// <summary>
        /// The cumulative concentration value.
        /// </summary>
        public double CumulativeConcentration { get; set; }
    }
}
