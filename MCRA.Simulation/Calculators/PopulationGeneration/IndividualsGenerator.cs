using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PopulationGeneration {
    public class IndividualsGenerator {

        public List<Individual> GenerateSimulatedIndividuals(
            Population population,
            int numberOfIndividuals,
            int numberOfDaysInsurvey,
            IRandom individualsRandomGenerator
        ) {
            var result = new List<Individual>();

            var individualProperties = population.PopulationIndividualPropertyValues;

            var ageProperty = new IndividualProperty() {
                Code = "Age",
                Name = "Age",
                PropertyType = IndividualPropertyType.Nonnegative,
                Min = 0,
                Max = 100,
            };
            var sexProperty = new IndividualProperty() {
                Code = "Gender",
                Name = "Gender",
                PropertyType = IndividualPropertyType.Gender,
                CategoricalLevels = ["male", "female"]
            };
            var bsaProperty = new IndividualProperty() {
                Code = "BSA",
                Name = "BSA",
                PropertyType = IndividualPropertyType.Nonnegative,
                Min = 0.25,
                Max = 3
            };

            var availableSexes = individualProperties
                .Where(r => r.Value.IndividualProperty.IsSexProperty())
                .SelectMany(r => r.Value.CategoricalLevels)
                .ToList();
            if (availableSexes.Count == 0) {
                availableSexes = ["male", "female"];
            }

            var availableAges = individualProperties
                .Where(r => r.Value.IndividualProperty.IsAgeProperty())
                .Select(r => Convert.ToInt32(r.Value.Value))
                .ToList();
            if (availableAges.Count == 0) {
                for (int i = (int)ageProperty.Min; i < (int)ageProperty.Max; i++) {
                    availableAges.Add(i);
                }
            }

            var availableBsa = individualProperties
                .Where(r => r.Value.IndividualProperty.Name == "BSA")
                .Select(r => Convert.ToDouble(r.Value.Value))
                .ToList();
            if (availableBsa.Count == 0) {
                for (double i = bsaProperty.Min; i < bsaProperty.Max;) {
                    availableBsa.Add(i);
                    i = i + 0.01;
                }
            }

            for (int i = 0; i < numberOfIndividuals; i++) {
                var age = availableAges.DrawRandom(individualsRandomGenerator);
                var sex = availableSexes.DrawRandom(individualsRandomGenerator);
                var bsa = availableBsa.DrawRandom(individualsRandomGenerator);
                var bwBirth = 3.68;
                var bw = bwBirth + (4.47 * age) - (0.093 * Math.Pow(age, 2D)) + (0.00061 * Math.Pow(age, 3D));
                var individualPropertyValues = new List<IndividualPropertyValue> {
                    new IndividualPropertyValue() {
                        IndividualProperty = ageProperty,
                        DoubleValue = age
                    },
                    new IndividualPropertyValue() {
                        IndividualProperty = sexProperty,
                        TextValue = sex
                    },
                    new IndividualPropertyValue() {
                        IndividualProperty = bsaProperty,
                        DoubleValue = bsa
                    }
                };
                var individual = new Individual(i) {
                    Code = $"{population.Code}-Ind{i}",
                    Name = $"{population.Code}-Ind{i}",
                    BodyWeight = bw,
                    NumberOfDaysInSurvey = numberOfDaysInsurvey,
                    IndividualPropertyValues = individualPropertyValues
                };
                result.Add(individual);
            }
            return result;
        }
    }
}
