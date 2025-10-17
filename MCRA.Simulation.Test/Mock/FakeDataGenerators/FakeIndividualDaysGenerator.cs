using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {

    /// <summary>
    /// Class for generating mock individual days
    /// </summary>
    public static class FakeIndividualDaysGenerator {

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
            var individuals = properties != null
                ? FakeIndividualsGenerator.Create(number, daysInSurvey, useSamplingWeights, properties, random)
                : FakeIndividualsGenerator.Create(number, daysInSurvey, random, useSamplingWeights);

            return Create(individuals);
        }

        public static List<SimulatedIndividualDay> CreateSimulatedIndividualDays(
            int number,
            int daysInSurvey,
            bool useSamplingWeights,
            IRandom random,
            List<IndividualProperty> properties = null
        ) {
            var individuals = FakeIndividualsGenerator.CreateSimulated(number, daysInSurvey, useSamplingWeights, random, properties);
            return CreateSimulatedIndividualDays(individuals);
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
                var simulatedIndividual = new SimulatedIndividual(individual, i);
                var days = group.ToList();
                for (int j = 0; j < days.Count; j++) {
                    result.Add(new SimulatedIndividualDay(simulatedIndividual) {
                        Day = days[j].IdDay,
                        SimulatedIndividualDayId = idx++,
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
        /// Creates a list of individual days from a list of <see cref="Individual"/>
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="replicates"></param>
        /// <returns></returns>
        public static List<SimulatedIndividualDay> CreateSimulatedIndividualDays(
            IEnumerable<Individual> individuals,
            int replicates = 1
        ) {
            var simulatedIndividuals = CreateSimulatedIndividuals(individuals);
            return CreateSimulatedIndividualDays(simulatedIndividuals, replicates);
        }

        /// <summary>
        /// Creates a list of simulated individuals
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="replicates"></param>
        /// <returns></returns>
        public static List<SimulatedIndividual> CreateSimulatedIndividuals(
            IEnumerable<Individual> individuals,
            int replicates = 1
        ) {
            var simIndex = 0;
            var sims = individuals
                .SelectMany(r => Enumerable.Repeat(r, replicates))
                .Select(r => new SimulatedIndividual(r, simIndex++) {
                    BodyWeight = r.BodyWeight,
                    Age = r.Age,
                    Gender = r.Gender
                })
                .ToList();
            return sims;
        }

        /// <summary>
        /// Creates a list of individual days
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="replicates"></param>
        /// <returns></returns>
        public static List<SimulatedIndividualDay> CreateSimulatedIndividualDays(
            IEnumerable<SimulatedIndividual> individuals,
            int replicates = 1
        ) {
            var individualDayCounter = 0;
            return individuals
                .SelectMany(r => Enumerable.Repeat(r, replicates))
                .SelectMany(r => Enumerable
                    .Range(0, r.NumberOfDaysInSurvey)
                    .Select((id, ix) =>
                        new SimulatedIndividualDay(r) {
                            SimulatedIndividualDayId = individualDayCounter++,
                            Day = ix.ToString()
                        })
                ).ToList();
        }
    }
}
