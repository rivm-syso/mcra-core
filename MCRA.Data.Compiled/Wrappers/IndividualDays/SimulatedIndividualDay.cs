namespace MCRA.Data.Compiled.Wrappers {

    /// <summary>
    /// Stores info about individual, the day and the simulated ID
    /// </summary>
    public sealed class SimulatedIndividualDay(SimulatedIndividual simulatedIndividual) : IIndividualDay {

        /// <summary>
        /// The source individual that is used for simulation
        /// </summary>
        public SimulatedIndividual SimulatedIndividual { get; } = simulatedIndividual;

        /// <summary>
        /// The source day that is used for simulation
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// The simulation id for this individual day
        /// </summary>
        public int SimulatedIndividualDayId { get; set; }
    }
}
