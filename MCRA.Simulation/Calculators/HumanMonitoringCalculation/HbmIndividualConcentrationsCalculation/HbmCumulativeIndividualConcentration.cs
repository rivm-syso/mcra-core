using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed class HbmCumulativeIndividualConcentration {
        /// <summary>
        /// The simulated individual id.
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// The original individual entity.
        /// </summary>
        public Individual Individual { get; set; }


        public double CumulativeConcentration { get ; set; }  
    }
}
