using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public class HbmCumulativeIndividualConcentration {

        /// <summary>
        /// The simulated individual id.
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// The original individual entity.
        /// </summary>
        public Individual Individual { get; set; }

        /// <summary>
        /// The cumulative concentration value.
        /// </summary>
        public double CumulativeConcentration { get ; set; }

        /// <summary>
        /// The sampling weight of the individual.
        /// </summary>
        public double IndividualSamplingWeight { get; set; } = 1D;
    }
}
