using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock individuals
    /// </summary>
    public static class MockIndividualsGenerator {

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
            var individualProperty = new IndividualProperty() {
                Code = "Age",
                Name = "Age",
                PropertyType = IndividualPropertyType.Nonnegative,
            };
            for (int i = 0; i < number; i++) {
                var individual = new Individual(i) {
                    Code = i.ToString(),
                    NumberOfDaysInSurvey = daysInSurvey,
                    BodyWeight = 75 + (double)((randomBodyWeight?.NextDouble() - 0.5) * 20 ?? 0),
                    SamplingWeight = useSamplingWeights ? random.NextDouble() * 5 : 1d,
                    CodeFoodSurvey = codeSurvey,
                    IndividualPropertyValues = new List<IndividualPropertyValue>() {
                        new IndividualPropertyValue() {
                            IndividualProperty = individualProperty,
                            DoubleValue = random.NextDouble() * 80,
                        }
                    }
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
    }
}
