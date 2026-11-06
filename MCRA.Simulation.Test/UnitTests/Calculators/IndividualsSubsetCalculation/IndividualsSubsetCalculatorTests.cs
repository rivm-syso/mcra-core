using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation.IndividualFilters;
using MCRA.Simulation.Calculators.PopulationDefinitionCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IndividualsSubsetCalculation {

    /// <summary>
    /// Individual subset selection calculator tests.
    /// </summary>
    [TestClass]
    public class IndividualsSubsetCalculatorTests {


        /// <summary>
        /// Test individuals identity with duplicates of gender.
        /// </summary>
        [TestMethod]
        public void Individual_TestGenderDuplicates() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(4, 2, random);

            var property = FakeIndividualPropertiesGenerator.FakeGenderProperty;
            var genderSurvey1 = new[] { "M", "M", "F", "F" };
            setIndividualPropertyTextValues(individuals, property, genderSurvey1);
            var genderSurvey2 = new[] { "M", "M", "F", "F" };
            setIndividualPropertyTextValues(individuals, property, genderSurvey2);
        }

        /// <summary>
        /// Test individuals identity with duplicates of age.
        /// </summary>
        [TestMethod]
        public void Individual_TestAgeDuplicates() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(4, 2, random);

            var property = FakeIndividualPropertiesGenerator.FakeAgeProperty;
            var ageSurvey1 = new double[] { 1, 2, 3, 4 };
            setIndividualPropertyNumericValues(individuals, property, ageSurvey1);
            var ageSurvey2 = new double[] { 1, 2, 3, 4 };
            setIndividualPropertyNumericValues(individuals, property, ageSurvey2);
        }


        /// <summary>
        /// Test individuals identity with duplicates of gender.
        /// </summary>
        [TestMethod]
        public void Individual_TestGenderNoDuplicates() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(4, 2, random);

            var property = FakeIndividualPropertiesGenerator.FakeGenderProperty;
            var genderSurvey1 = new[] { "M", "M", "F", "F" };
            setIndividualPropertyTextValues(individuals, property, genderSurvey1);
            var genderSurvey2 = new[] { "F", "F", "M", "M" };
            var exception = Assert.Throws<Exception>(() => setIndividualPropertyTextValues(individuals, property, genderSurvey2));
            Assert.AreEqual("Gender property already set for individual.", exception.Message);
        }

        /// <summary>
        /// Test individuals identity with duplicates of age.
        /// </summary>
        [TestMethod]
        public void Individual_TestAgeNoDuplicates() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(4, 2, random);

            var property = FakeIndividualPropertiesGenerator.FakeAgeProperty;
            var ageSurvey1 = new double[] { 1, 2, 3, 4 };
            setIndividualPropertyNumericValues(individuals, property, ageSurvey1);
            var ageSurvey2 = new double[] { 8, 9, 10, 11 };
            var exception = Assert.Throws<Exception>(() => setIndividualPropertyNumericValues(individuals, property, ageSurvey2));
            Assert.AreEqual("Age property already set for individual.", exception.Message);
        }

        /// <summary>
        /// Test calculation of individuals subset by gender.
        /// </summary>
        [TestMethod]
        public void IndividualSubsetFilter_TestGenderSubset() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(10, 2, random);

            var property = FakeIndividualPropertiesGenerator.FakeGenderProperty;
            var values = new[] { "M", "M", "M", "m", "F", "f", "F", "F", "F", "F" };
            setIndividualPropertyTextValues(individuals, property, values);

            var subsetDefinition = new IndividualsSubsetDefinition("gender", "'F'");
            var filters = createFilters([property], [subsetDefinition]);

            var subset = IndividualsSubsetCalculator.ComputeIndividualsSubset(
                individuals,
                filters
            );

            Assert.HasCount(6, subset);
        }

        /// <summary>
        /// Test calculation of individuals subset by age.
        /// </summary>
        [TestMethod]
        public void IndividualSubsetFilter_TestAgeSubset() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(10, 2, random);

            var property = FakeIndividualPropertiesGenerator.FakeAgeProperty;
            var values = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            setIndividualPropertyNumericValues(individuals, property, values);

            var subsetDefinition = new IndividualsSubsetDefinition("age", "2-4");
            var filters = createFilters([property], [subsetDefinition]);

            var subset = IndividualsSubsetCalculator.ComputeIndividualsSubset(
                individuals,
                filters
            );

            Assert.HasCount(3, subset);
        }

        /// <summary>
        /// Test calculation of individuals subset by age (upper bound only).
        /// </summary>
        [TestMethod]
        public void IndividualSubsetFilter_TestAgeUpperBoundSubset() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(10, 2, random);

            var property = FakeIndividualPropertiesGenerator.FakeAgeProperty;
            var values = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            setIndividualPropertyNumericValues(individuals, property, values);

            var subsetDefinition = new IndividualsSubsetDefinition("age", "-4");
            var filters = createFilters([property], [subsetDefinition]);

            var subset = IndividualsSubsetCalculator.ComputeIndividualsSubset(
                individuals,
                filters
            );

            Assert.HasCount(4, subset);
        }

        /// <summary>
        /// Test calculation of individuals subset by age (upper bound only).
        /// </summary>
        [TestMethod]
        public void IndividualSubsetFilter_TestAgeLowerBoundSubset() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(10, 2, random);

            var property = FakeIndividualPropertiesGenerator.FakeAgeProperty;
            var values = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            setIndividualPropertyNumericValues(individuals, property, values);

            var subsetDefinition = new IndividualsSubsetDefinition("age", "4-");
            var filters = createFilters([property], [subsetDefinition]);

            var subset = IndividualsSubsetCalculator.ComputeIndividualsSubset(
                individuals,
                filters
            );

            Assert.HasCount(7, subset);
        }

        /// <summary>
        /// Test calculation of individuals subset by age and gender.
        /// </summary>
        [TestMethod]
        public void IndividualSubsetFilter_TestAgeAndGenderSubset() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(10, 2, random);

            var genderProperty = FakeIndividualPropertiesGenerator.FakeGenderProperty;
            var genderValues = new[] { "M", "M", "M", "m", "F", "f", "F", "F", "F", "F" };
            setIndividualPropertyTextValues(individuals, genderProperty, genderValues);

            var ageProperty = FakeIndividualPropertiesGenerator.FakeAgeProperty;
            var ageValues = new double[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            setIndividualPropertyNumericValues(individuals, ageProperty, ageValues);

            var ageSubset = new IndividualsSubsetDefinition(ageProperty.Code, "3-6");
            var sexSubset = new IndividualsSubsetDefinition(genderProperty.Code, "'F'");
            var filters = createFilters(
                [ageProperty, genderProperty],
                [ageSubset, sexSubset]
            );

            var subset = IndividualsSubsetCalculator.ComputeIndividualsSubset(
                individuals,
                filters
            );

            Assert.HasCount(2, subset);
        }

        private static void setIndividualPropertyTextValues(
            List<Individual> individuals,
            IndividualProperty property,
            string[] values
        ) {
            for (int i = 0; i < individuals.Count; i++) {
                individuals[i].SetPropertyValue(property, textValue: values[i]);
            }
        }

        private static void setIndividualPropertyNumericValues(
            List<Individual> individuals,
            IndividualProperty property,
            double[] values
        ) {
            for (int i = 0; i < individuals.Count; i++) {
                individuals[i].SetPropertyValue(property, doubleValue: values[i]);
            }
        }

        private static ICollection<IPropertyIndividualFilter> createFilters(
            ICollection<IndividualProperty> properties,
            List<IndividualsSubsetDefinition> subsetDefinitions,
            IndividualSubsetType individualSubsetType = IndividualSubsetType.MatchToPopulationDefinition,
            ICollection<string> selectedSurveySubsetProperties = null
        ) {
            var populationBuilder = new PopulationDefinitionBuilder();
            var population = populationBuilder.Create(
                double.NaN,
                true,
                subsetDefinitions,
                null
            );

            // Create individual (subset) filters
            var individualsSubsetCalculator = new IndividualsSubsetFiltersBuilder();
            var filters = individualsSubsetCalculator
                .Create(
                    population,
                    properties,
                    individualSubsetType,
                    selectedSurveySubsetProperties
                );
            return filters;
        }
    }
}
