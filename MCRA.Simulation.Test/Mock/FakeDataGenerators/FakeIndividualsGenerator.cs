using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Objects;
using MCRA.General;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock individuals
    /// </summary>
    public static class FakeIndividualsGenerator {

        /// <summary>
        /// Creates a single individual.
        /// </summary>
        /// <param name="bodyWeight"></param>
        /// <returns></returns>
        public static SimulatedIndividual CreateSingle(
            double bodyWeight = 70
        ) {
            var individual = new SimulatedIndividual(
                new(0) { BodyWeight = bodyWeight }, 0
            );
            return individual;
        }

        /// <summary>
        /// Creates a list of individuals with a fixed seed
        /// </summary>
        public static List<Individual> Create(
            int number,
            int daysInSurvey,
            IRandom random,
            bool useSamplingWeights = false,
            string codeSurvey = null,
            IRandom randomBodyWeight = null
        ) {
            var individuals = new List<Individual>();
            for (int i = 0; i < number; i++) {
                var individual = new Individual(i) {
                    Code = i.ToString(),
                    NumberOfDaysInSurvey = daysInSurvey,
                    BodyWeight = 75 + (double)((randomBodyWeight?.NextDouble() - 0.5) * 20 ?? 0),
                    SamplingWeight = useSamplingWeights ? random.NextDouble() * 5 : 1d,
                    CodeFoodSurvey = codeSurvey
                };
                individuals.Add(individual);
            }
            return individuals;
        }

        /// <summary>
        /// Creates a list of simulated individuals with a fixed seed
        /// </summary>
        public static List<SimulatedIndividual> CreateSimulated(
            int number,
            int daysInSurvey,
            IRandom random,
            bool useSamplingWeights = false,
            string codeSurvey = null,
            IRandom randomBodyWeight = null
        ) {
            var individuals = Create(number, daysInSurvey, random, useSamplingWeights, codeSurvey, randomBodyWeight);
            return CreateSimulated(individuals);
        }

        /// <summary>
        /// Creates a list of simulated individuals
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="replicates"></param>
        /// <returns></returns>
        public static List<SimulatedIndividual> CreateSimulated(
            IEnumerable<Individual> individuals,
            int replicates = 1
        ) {
            var simIndex = 0;
            int getIndex() => simIndex++;

            var sims = individuals
                .SelectMany(r => Enumerable.Repeat(r, replicates))
                .Select(id => new SimulatedIndividual(id, getIndex()))
                .ToList();
            return sims;
        }

        public static List<SimulatedIndividual> CreateSimulated(
            int number,
            int daysInSurvey,
            bool useSamplingWeights,
            IRandom random,
            List<IndividualProperty> properties = null
        ) {
            var individuals = properties != null
                ? Create(number, daysInSurvey, useSamplingWeights, properties, random)
                : Create(number, daysInSurvey, random, useSamplingWeights);

            return CreateSimulated(individuals);
        }

        /// <summary>
        /// Creates a list of individuals with propertiers with a fixed seed
        /// </summary>
        /// <param name="number"></param>
        /// <param name="daysInSurvey"></param>
        /// <param name="useSamplingWeights"></param>
        /// <param name="properties"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static List<Individual> Create(
            int number,
            int daysInSurvey,
            bool useSamplingWeights,
            List<IndividualProperty> properties,
            IRandom random
        ) {
            var individuals = new List<Individual>();
            var cofactor = properties.First(c => c.PropertyType.GetPropertyType() == PropertyType.Cofactor);
            var covariable = properties.First(c => c.PropertyType.GetPropertyType() == PropertyType.Covariable);

            for (int i = 0; i < number; i++) {
                var individualProperties = new List<IndividualPropertyValue>();
                var individual = new Individual(i) {
                    NumberOfDaysInSurvey = daysInSurvey,
                    BodyWeight = 75,
                    SamplingWeight = useSamplingWeights ? random.NextDouble() * 5 : 1d,
                    Code = i.ToString(),
                };

                foreach (var property in properties) {
                    if (property.PropertyType == IndividualPropertyType.Numeric
                        || property.PropertyType == IndividualPropertyType.Nonnegative
                        || property.PropertyType == IndividualPropertyType.Integer
                        || property.PropertyType == IndividualPropertyType.NonnegativeInteger
                    ) {
                        var doubleValue = Math.Floor(random.NextDouble(property.Min, property.Max));
                        individual.SetPropertyValue(property, doubleValue: doubleValue);

                        if (property == covariable) {
                            individual.Covariable = doubleValue;
                        }
                    } else {
                        var textValue = property.CategoricalLevels.ElementAt(i % property.CategoricalLevels.Count);
                        individual.SetPropertyValue(property, textValue: textValue);
                        if (property == cofactor) {
                            individual.Cofactor = textValue;
                        }
                    }
                }
                individuals.Add(individual);
            }
            return individuals;
        }

        /// <summary>
        /// Adds a random sex property value to each individual of the collection.
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="random"></param>
        public static void AddFakeSexProperty(
            List<Individual> individuals,
            IRandom random
        ) {
            var sexIndividualProperty = FakeIndividualPropertiesGenerator.FakeGenderProperty;
            foreach (var individual in individuals) {
                individual.SetPropertyValue(
                    sexIndividualProperty,
                    textValue: random.NextDouble() > .5
                        ? GenderType.Male.ToString()
                        : GenderType.Female.ToString()
                );
            }
        }

        /// <summary>
        /// Adds a random age property value to each individual of the collection.
        /// </summary>
        /// <param name="individuals"></param>
        /// <param name="random"></param>
        public static void AddFakeAgeProperty(
            List<Individual> individuals,
            IRandom random,
            double min = 0,
            double max = 100
        ) {
            var individualProperty = FakeIndividualPropertiesGenerator.FakeAgeProperty;
            foreach (var individual in individuals) {
                individual.SetPropertyValue(
                    individualProperty,
                    doubleValue: Math.Round(min + (max - min) * random.NextDouble())
                );
            }
        }
    }
}
