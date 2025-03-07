namespace MCRA.Data.Compiled.Wrappers {

    /// <summary>
    /// Stores info about individual, the day and the simulated day ID
    /// </summary>
    public interface IIndividualDay {

        /// <summary>
        /// The individual that is used for simulation
        /// </summary>
        SimulatedIndividual SimulatedIndividual { get; }

        /// <summary>
        /// The source day that is used for simulation
        /// </summary>
        string Day { get; set; }

        /// <summary>
        /// The simulation id for this individual day
        /// </summary>
        int SimulatedIndividualDayId { get; set; }
    }
}
