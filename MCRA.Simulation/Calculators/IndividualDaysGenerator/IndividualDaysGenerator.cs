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

        public static void ImputeBodyWeight(
           ICollection<SimulatedIndividualDay> simulatedIndividualDays,
           bool imputeIndividuals = false
        ) {
            var averageBodyWeight = SimulationConstants.DefaultBodyWeight;
            var allBodyWeights = simulatedIndividualDays
                .Select(r => r.Individual)
                .Distinct()
                .Where(r => !double.IsNaN(r.BodyWeight))
                .Select(r => r.BodyWeight);

            if(allBodyWeights.Any()) {
                averageBodyWeight = allBodyWeights.Average();
            }

            foreach (var d in simulatedIndividualDays) {
                if (double.IsNaN(d.Individual.BodyWeight)) {
                    d.IndividualBodyWeight = averageBodyWeight;
                    //fix #2054: dietary exposures use Individual.BodyWeight
                    // where an imputed value should be used, solve that by
                    // allowing to impute the value in the Individual.BodyWeight itself.
                    //TODO: refactor, use a new SimulatedIndividual type for imputation etc (see #2089)
                    if (imputeIndividuals) {
                        d.Individual.BodyWeight = averageBodyWeight;
                    }
                } else {
                    d.IndividualBodyWeight = d.Individual.BodyWeight;
                }
            }
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
