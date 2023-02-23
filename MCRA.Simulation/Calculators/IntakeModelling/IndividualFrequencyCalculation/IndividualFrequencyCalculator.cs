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
                .GroupBy(idi => idi.SimulatedIndividualId)
                .Select(g => new IndividualFrequency() {
                    SimulatedIndividualId = g.Key,
                    Cofactor = g.Select(idi => idi.Individual.Cofactor).First(),
                    Covariable = g.Select(idi => idi.Individual.Covariable).First(),
                    Nbinomial = g.Count(),
                    Frequency = g.Count(idi => idi.Amount > 0),
                    NumberOfIndividuals = 1,
                    SamplingWeight = g.First().IndividualSamplingWeight,
                })
                .ToList();
        }
    }
}
