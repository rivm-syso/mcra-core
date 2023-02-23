using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock simple individual intakes.
    /// </summary>
    public class MockSimpleIndividualIntakeGenerator {

        /// <summary>
        /// Generates individual day amounts.
        /// </summary>
        /// <param name="simulatedIndividuals"></param>
        /// <returns></returns>
        public static ICollection<SimpleIndividualIntake> Create(
            ICollection<SimpleIndividualDayIntake> simulatedIndividuals
        ) {
            var result = simulatedIndividuals.GroupBy(idi => idi.SimulatedIndividualId)
                .Select(g => {
                    var individualIntakes = g
                        .Select(idi => idi.Amount)
                        .ToArray();
                    var numPositiveIntakeDays = individualIntakes.Where(r => r > 0).Count();
                    var totalIntake = individualIntakes.Sum();
                    var individual = g.First().Individual;
                    return new SimpleIndividualIntake() {
                        SimulatedIndividualId = g.Key,
                        Cofactor = individual.Cofactor,
                        Covariable = individual.Covariable,
                        NumberOfDays = g.Count(),
                        NumberOfPositiveIntakeDays = numPositiveIntakeDays,
                        Intake = totalIntake / numPositiveIntakeDays,
                        DayIntakes = individualIntakes,
                        IndividualSamplingWeight = g.First().IndividualSamplingWeight,
                    };
                })
                .ToList();
            return result;
        }
    }
}
