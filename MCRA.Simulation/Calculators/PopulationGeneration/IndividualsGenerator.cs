﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Objects;
using MCRA.Utils.ExtensionMethods;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Calculators.PopulationGeneration {
    public class IndividualsGenerator {

        private readonly bool _allometricScaling = true;

        public List<SimulatedIndividual> GenerateSimulatedIndividuals(
            Population population,
            int numberOfIndividuals,
            int numberOfDaysInsurvey,
            IRandom individualsRandomGenerator
        ) {
            var result = new List<SimulatedIndividual>();

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
                .Where(r => r.Value.IndividualProperty.IsSexProperty)
                .SelectMany(r => r.Value.CategoricalLevels)
                .ToList();
            if (availableSexes.Count == 0) {
                availableSexes = ["male", "female"];
            }

            var populationPropertyAge = individualProperties
                .SingleOrDefault(r => r.Value.IndividualProperty.IsAgeProperty)
                .Value;
            var minimumAge = (int)(populationPropertyAge?.MinValue ?? ageProperty.Min);
            var maximumAge = (int)(populationPropertyAge?.MaxValue ?? ageProperty.Max);
            var availableAges = Enumerable.Range(minimumAge, maximumAge - minimumAge + 1).ToList();

            var populationPropertyBsa = individualProperties
                .SingleOrDefault(r => r.Value.IndividualProperty.Name == "BSA")
                .Value;
            var minimumBsa = populationPropertyBsa?.MinValue ?? bsaProperty.Min;
            var maximumBsa = populationPropertyBsa?.MaxValue ?? bsaProperty.Max;
            var availableBsa = new List<double>();
            for (double i = minimumBsa; i < maximumBsa;) {
                availableBsa.Add(i);
                i = i + 0.01;
            }

            for (int i = 0; i < numberOfIndividuals; i++) {
                var age = availableAges.DrawRandom(individualsRandomGenerator);
                var sex = availableSexes.DrawRandom(individualsRandomGenerator);
                var bwBirth = 3.68;
                var bw = bwBirth + (4.47 * age) - (0.093 * Math.Pow(age, 2D)) + (0.00061 * Math.Pow(age, 3D));
                var bsa = _allometricScaling
                    ? getAllometricScaledBSA(bw)
                    : availableBsa.DrawRandom(individualsRandomGenerator);
                var individual = new Individual(i) {
                    Code = $"{population.Code}-Ind{i}",
                    Name = $"{population.Code}-Ind{i}",
                    BodyWeight = bw,
                    NumberOfDaysInSurvey = numberOfDaysInsurvey
                };
                individual.SetPropertyValue(ageProperty, doubleValue: age);
                individual.SetPropertyValue(sexProperty, sex);
                individual.SetPropertyValue(bsaProperty, doubleValue: bsa);
                result.Add(new(individual, i));
            }
            return result;
        }

        // Allometric scaling Bokkers and Slob, 2007
        // BSA is in dm2??
        private static double getAllometricScaledBSA(double bodyWeight) {
            var standardBodyWeight = 70d;
            var standardBSA = 270d;
            var bsaScaling = Math.Pow(standardBodyWeight / bodyWeight, 1 - 0.7);
            var bsa = standardBSA / bsaScaling / 100;
            return bsa;
        }
    }
}
