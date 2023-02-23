using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {

    /// <summary>
    /// Class for generating mock individual days
    /// </summary>
    public static class MockIndividualDaysGenerator {

        /// <summary>
        /// Creates a list of individual days
        /// </summary>
        /// <param name="number"></param>
        /// <param name="daysInSurvey"></param>
        /// <param name="useSamplingWeights"></param>
        /// <param name="random"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static List<IndividualDay> Create(
            int number,
            int daysInSurvey,
            bool useSamplingWeights,
            IRandom random,
            List<IndividualProperty> properties = null
        ) {
            if (properties != null) {
                var individuals = MockIndividualsGenerator.Create(number, daysInSurvey, useSamplingWeights, properties, random);
                return Create(individuals);
            } else {
                var individuals = MockIndividualsGenerator.Create(number, daysInSurvey, random, useSamplingWeights);
                return Create(individuals);
            }
        }

        public static List<SimulatedIndividualDay> CreateSimulatedIndividualDays(
            int number,
            int daysInSurvey,
            bool useSamplingWeights,
            IRandom random,
            List<IndividualProperty> properties = null
        ) {
            if (properties != null) {
                var individuals = MockIndividualsGenerator.Create(number, daysInSurvey, useSamplingWeights, properties, random);
                return CreateSimulatedIndividualDays(individuals);
            } else {
                var individuals = MockIndividualsGenerator.Create(number, daysInSurvey, random, useSamplingWeights);
                return CreateSimulatedIndividualDays(individuals);
            }
        }

        public static List<SimulatedIndividualDay> CreateSimulatedIndividualDays(
            ICollection<IndividualDay> individualDays
        ) {
            var result = new List<SimulatedIndividualDay>();
            var grouping = individualDays.GroupBy(r => r.Individual);
            var idx = 0;
            for (int i = 0; i < grouping.Count(); i++) {
                var group = grouping.ElementAt(i);
                var individual = grouping.ElementAt(i).Key;
                var days = group.ToList();
                for (int j = 0; j < days.Count; j++) {
                    result.Add(new SimulatedIndividualDay() {
                        Individual = individual,
                        Day = days[j].IdDay,
                        SimulatedIndividualId = i,
                        SimulatedIndividualDayId = idx++,
                        IndividualSamplingWeight = individual.SamplingWeight
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// Creates a list of individuals
        /// </summary>
        /// <param name="individuals"></param>
        /// <returns></returns>
        public static List<IndividualDay> Create(
            ICollection<Individual> individuals
        ) {
            var result = new List<IndividualDay>();
            foreach (var individual in individuals) {
                individual.IndividualDays = new Dictionary<string, IndividualDay>(StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < individual.NumberOfDaysInSurvey; i++) {
                    var idDay = i.ToString();
                    var individualDay = new IndividualDay() {
                        Individual = individual,
                        IdDay = idDay,
                    };
                    result.Add(individualDay);
                    individual.IndividualDays[i.ToString()] = individualDay;

                }
            }
            return result;
        }

        /// <summary>
        /// Creates a list of individuals
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="replicates"></param>
        /// <returns></returns>
        public static List<SimulatedIndividualDay> CreateSimulatedIndividualDays(
            ICollection<Individual> individuals,
            int replicates = 1
        ) {
            var individualDayCounter = 0;
            return individuals
                .SelectMany(r => Enumerable.Repeat(r, replicates))
                .SelectMany(r => Enumerable
                .Range(0, r.NumberOfDaysInSurvey)
                .Select((id, ix) =>
                    new SimulatedIndividualDay() {
                        SimulatedIndividualId = r.Id,
                        SimulatedIndividualDayId = individualDayCounter++,
                        IndividualSamplingWeight = r.SamplingWeight,
                        Individual = r,
                        Day = ix.ToString(),
                    })
                )
                .ToList();
        }
    }
}
