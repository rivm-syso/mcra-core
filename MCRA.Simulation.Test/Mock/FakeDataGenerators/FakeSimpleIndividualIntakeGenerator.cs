using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock simple individual intakes.
    /// </summary>
    public class FakeSimpleIndividualIntakeGenerator {

        /// <summary>
        /// Generates individual day amounts.
        /// </summary>
        /// <param name="simulatedIndividuals"></param>
        /// <returns></returns>
        public static ICollection<SimpleIndividualIntake> Create(
            ICollection<SimpleIndividualDayIntake> simulatedIndividuals
        ) {
            var result = simulatedIndividuals.GroupBy(idi => idi.SimulatedIndividual)
                .Select(g => {
                    var individualIntakes = g
                        .Select(idi => idi.Amount)
                        .ToArray();
                    var numPositiveIntakeDays = individualIntakes.Count(r => r > 0);
                    var totalIntake = individualIntakes.Sum();
                    return new SimpleIndividualIntake(g.Key) {
                        Cofactor = g.Key.Cofactor,
                        Covariable = g.Key.Covariable,
                        NumberOfDays = g.Count(),
                        NumberOfPositiveIntakeDays = numPositiveIntakeDays,
                        Intake = totalIntake / numPositiveIntakeDays,
                        DayIntakes = individualIntakes,
                    };
                })
                .ToList();
            return result;
        }
    }
}
