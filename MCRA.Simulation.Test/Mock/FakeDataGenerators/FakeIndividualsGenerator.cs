using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

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
        public static Individual CreateSingle(
            double bodyWeight = 70
        ) {
            var individual = new Individual(0) {
                BodyWeight = bodyWeight,
            };
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
                    CodeFoodSurvey = codeSurvey,
                    IndividualPropertyValues = []
                };
                individuals.Add(individual);
            }
            return individuals;
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
                        var individualPropertyValue = new IndividualPropertyValue() {
                            IndividualProperty = property,
                            DoubleValue = Math.Floor(random.NextDouble(property.Min, property.Max)),
                        };
                        individual.IndividualPropertyValues.Add(individualPropertyValue);
                        if (property == covariable) {
                            individual.Covariable = individualPropertyValue.DoubleValue.Value;
                        }
                    } else {
                        var individualPropertyValue = new IndividualPropertyValue() {
                            IndividualProperty = property,
                            TextValue = property.CategoricalLevels.ElementAt(i % property.CategoricalLevels.Count)
                        };
                        individual.IndividualPropertyValues.Add(individualPropertyValue);
                        if (property == cofactor) {
                            individual.Cofactor = individualPropertyValue.TextValue;
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
                individual.IndividualPropertyValues.Add(new() {
                    IndividualProperty = sexIndividualProperty,
                    TextValue = random.NextDouble() > .5 ? GenderType.Male.ToString() : GenderType.Female.ToString(),
                });
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
                individual.IndividualPropertyValues.Add(new() {
                    IndividualProperty = individualProperty,
                    DoubleValue = Math.Round(min + (max - min) * random.NextDouble()),
                });
            }
        }
    }
}
