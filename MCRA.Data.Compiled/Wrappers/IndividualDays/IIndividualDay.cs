using MCRA.Data.Compiled.Objects;

namespace MCRA.Data.Compiled.Wrappers {

    /// <summary>
    /// Stores info about individual, the day and the simulated ID
    /// </summary>
    public interface IIndividualDay {

        /// <summary>
        /// The source individual that is used for simulation
        /// </summary>
        Individual Individual { get; set; }

        /// <summary>
        /// The source day that is used for simulation
        /// </summary>
        string Day { get; set; }

        /// <summary>
        /// The simulation id for this individual
        /// </summary>
        int SimulatedIndividualId { get; set; }

        /// <summary>
        /// The simulation id for this individual day
        /// </summary>
        int SimulatedIndividualDayId { get; set; }

        /// <summary>
        /// The sampling weight of the (simulated) individual.
        /// </summary>
        double IndividualSamplingWeight { get; set; }
    }
}
