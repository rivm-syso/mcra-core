using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
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

        public static IEnumerable<SimulatedIndividualDay> ImputeBodyWeight(
           ICollection<SimulatedIndividualDay> simulatedIndividualDays
        ) {
            var allBodyWeights = simulatedIndividualDays
                .Select(r => r.Individual)
                .Distinct()
                .Where(r => !double.IsNaN(r.BodyWeight))
                .Select(r => r.BodyWeight)
                .ToList();
            var averageBodyWeight = allBodyWeights.Count == 0 ? SimulationConstants.DefaultBodyWeight : allBodyWeights.Average();

            var result = simulatedIndividualDays
                .Select(r => {
                    r.IndividualBodyWeight = double.IsNaN(r.Individual.BodyWeight) ? averageBodyWeight : r.Individual.BodyWeight;
                    return r;
                });

            return result;
        }

        public static List<IIndividualDay> CreateSimulatedIndividualDays(
            ICollection<Individual> individuals
        ) {
            var result = individuals
                .SelectMany(
                    i => Enumerable.Range(0, i.NumberOfDaysInSurvey),
                    (i, d) => new SimulatedIndividualDay() {
                        Individual = i,
                        Day = $"{d}",
                        SimulatedIndividualId = i.Id,
                        SimulatedIndividualDayId = i.Id * d + d,
                        IndividualSamplingWeight = i.SamplingWeight
                    })
                .Cast<IIndividualDay>()
                .ToList();
            return result;
        }
    }
}
