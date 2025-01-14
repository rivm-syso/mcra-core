using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Represents a simple individual day intake, with an intake amount
    /// specified by one double.
    /// </summary>
    public class SimpleIndividualDayIntake : IIndividualDay {
        /// <summary>
        /// The individual of this individual day amount.
        /// </summary>
        public SimulatedIndividual SimulatedIndividual { get; set; }

        /// <summary>
        /// Identifier of the simulated individual day.
        /// </summary>
        public int SimulatedIndividualDayId { get; set; }

        /// <summary>
        /// The survey day of this individual day amount.
        /// </summary>
        public string Day { get; set; }

        /// <summary>
        /// Intake amount.
        /// </summary>
        public double Amount { get; set; }
    }
}
