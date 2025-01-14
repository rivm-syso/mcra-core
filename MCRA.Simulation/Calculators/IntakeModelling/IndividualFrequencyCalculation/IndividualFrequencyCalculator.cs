namespace MCRA.Simulation.Calculators.IntakeModelling {

    /// <summary>
    /// Calculator for computing individual frequencies from individual-day
    /// intakes.
    /// </summary>
    public class IndividualFrequencyCalculator {

        /// <summary>
        /// Computes the individual frequencies from the collection of dietary
        /// individual day intakes.
        /// </summary>
        /// <param name="individualDayIntakeAmounts"></param>
        /// <returns></returns>
        public static ICollection<IndividualFrequency> Compute(
            ICollection<SimpleIndividualDayIntake> individualDayIntakeAmounts
        ) {
            return individualDayIntakeAmounts
                .GroupBy(idi => idi.SimulatedIndividual)
                .Select(g => new IndividualFrequency(g.Key) {
                    Cofactor = g.Key.Cofactor,
                    Covariable = g.Key.Covariable,
                    Nbinomial = g.Count(),
                    Frequency = g.Count(idi => idi.Amount > 0),
                    NumberOfIndividuals = 1
                })
                .ToList();
        }
    }
}
