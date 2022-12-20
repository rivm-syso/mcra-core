namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Summarizes all amount info for a individual
    /// </summary>
    public class SimpleIndividualIntake {

        /// <summary>
        /// Identifier of the simulated individual.
        /// </summary>
        public int SimulatedIndividualId { get; set; }

        /// <summary>
        /// Sampling weight of the simulated individual.
        /// </summary>
        public double IndividualSamplingWeight { get; set; }

        /// <summary>
        /// Cofactor value of the individual.
        /// </summary>
        public string Cofactor { get; set; }

        /// <summary>
        /// Covariable value of the individual.
        /// </summary>
        public double Covariable { get; set; }

        /// <summary>
        /// Intake amount.
        /// </summary>
        public double Intake { get; set; }

        /// <summary>
        /// The individual-day amounts.
        /// </summary>
        public double[] DayIntakes { get; set; }

        /// <summary>
        /// Number of days recorded for the individual.
        /// </summary>
        public int NumberOfDays { get; set; }

        /// <summary>
        /// Number of positive intake days.
        /// </summary>
        public int NumberOfPositiveIntakeDays { get; set; }
    }
}
