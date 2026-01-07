using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Simulation.Constants;

namespace MCRA.Simulation.Calculators.IndividualDaysGenerator {
    public class IndividualDaysGenerator {

        public static HashSet<IndividualDay> IncludeEmptyIndividualDays(
            ICollection<IndividualDay> individualDays,
            ICollection<Individual> individuals
        ) {
            var result = individualDays.ToHashSet();
            var availableIndividualDays = individualDays.ToLookup(r => r.Individual);
            foreach (var individual in individuals) {
                var daysCount = availableIndividualDays[individual].Count();
                var emptyDays = individual.NumberOfDaysInSurvey - daysCount;
                for (int i = 0; i < emptyDays; i++) {
                    result.Add(new IndividualDay() {
                        Individual = individual,
                        IdDay = $"EmptyDay {i}"
                    });
                }
            }
            return result;
        }

        public static void ImputeBodyWeight(IEnumerable<SimulatedIndividual> simulatedIndividuals) {
            var averageBodyWeight = SimulationConstants.DefaultBodyWeight;

            var allBodyWeights = simulatedIndividuals
                .Where(r => !r.MissingBodyWeight)
                .Select(r => r.BodyWeight);

            if (allBodyWeights.Any()) {
                averageBodyWeight = allBodyWeights.Average();
            }

            foreach (var d in simulatedIndividuals.Where(id => id.MissingBodyWeight)) {
                d.BodyWeight = averageBodyWeight;
            }
        }

        public static List<IIndividualDay> CreateSimulatedIndividualDays(
            ICollection<SimulatedIndividual> individuals
        ) {
            var individualDayCount = 0;
            var result = individuals
                .SelectMany(
                    i => Enumerable.Range(0, i.NumberOfDaysInSurvey),
                    (i, d) => (IIndividualDay)new SimulatedIndividualDay(i) {
                        Day = $"{d}",
                        SimulatedIndividualDayId = individualDayCount++,
                    })
                .ToList();
            return result;
        }
    }
}
