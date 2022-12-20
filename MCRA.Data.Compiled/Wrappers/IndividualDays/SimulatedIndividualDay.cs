using MCRA.Data.Compiled.Objects;

namespace MCRA.Data.Compiled.Wrappers {

    /// <summary>
    /// Stores info about individual, the day and the simulated ID
    /// </summary>
    public sealed class SimulatedIndividualDay : IIndividualDay {

        /// <summary>
        /// The source individual that is used for simulation
        /// </summary>
        public Individual Individual { get; set; }

        /// <summary>
        /// The source day that is used for simulation
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// The simulation id for this individual
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// The simulation id for this individual day
        /// </summary>
        public int SimulatedIndividualDayId { get; set; }

        /// <summary>
        /// The sampling weight of the (simulated) individual.
        /// </summary>
        public double IndividualSamplingWeight { get; set; }
    }
}
