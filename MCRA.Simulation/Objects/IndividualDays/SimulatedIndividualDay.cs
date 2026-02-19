namespace MCRA.Simulation.Objects {

    /// <summary>
    /// Stores info about individual, the day and the simulated ID
    /// </summary>
    public sealed class SimulatedIndividualDay : IIndividualDay {

        public SimulatedIndividualDay(SimulatedIndividual simulatedIndividual) {
            SimulatedIndividual = simulatedIndividual;
        }

        public SimulatedIndividualDay(
            SimulatedIndividual simulatedIndividual,
            int simulatedIndividualDayId
        ) {
            SimulatedIndividual = simulatedIndividual;
            SimulatedIndividualDayId = simulatedIndividualDayId;
        }

        /// <summary>
        /// The source individual that is used for simulation
        /// </summary>
        public SimulatedIndividual SimulatedIndividual { get; }

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
