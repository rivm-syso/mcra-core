using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation {
    public sealed  class HbmCumulativeIndividualDayConcentration {
        /// <summary>
        /// The simulated individual id.
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// The simulated individual day id.
        /// </summary>
        public string SimulatedIndividualDayId {
            get {
                return $"{SimulatedIndividualId}{Day}";
            }
        }

        /// <summary>
        /// The original individual entity.
        /// </summary>
        public Individual Individual { get; set; }

        public double CumulativeConcentration { get; set; }

        /// <summary>
        /// The (survey)day of the exposure.
        /// </summary>
        public string Day { get; set; }
    }
}
