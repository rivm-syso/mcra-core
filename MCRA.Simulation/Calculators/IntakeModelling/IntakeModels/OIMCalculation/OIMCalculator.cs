using MCRA.Simulation.Calculators.DietaryExposuresCalculation.IndividualDietaryExposureCalculation;

namespace MCRA.Simulation.Calculators.IntakeModelling.IntakeModels.OIMCalculation {

    /// <summary>
    /// Observed Individual Mean model for chronic exposure assessment
    /// </summary>
    public class OIMCalculator {

        /// <summary>
        /// Calculate observed individual means
        /// </summary>
        /// <param name="simpleIndividualDayIntakes"></param>
        /// <returns></returns>
        public static List<DietaryIndividualIntake> CalculateObservedIndividualMeans(
            ICollection<SimpleIndividualDayIntake> simpleIndividualDayIntakes
        ) {
            return simpleIndividualDayIntakes
                .GroupBy(r => r.SimulatedIndividual)
                .Select(g => new DietaryIndividualIntake() {
                    SimulatedIndividual = g.Key,
                    NumberOfDays = g.Count(idi => idi.Amount > 0),
                    DietaryIntakePerMassUnit = g.Average(i => i.Amount),
                })
                .OrderBy(o => o.DietaryIntakePerMassUnit)
                .ToList();
        }
    }
}
