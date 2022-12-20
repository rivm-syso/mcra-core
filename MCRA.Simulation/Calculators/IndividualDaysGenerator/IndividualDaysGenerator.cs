using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

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
    }
}
