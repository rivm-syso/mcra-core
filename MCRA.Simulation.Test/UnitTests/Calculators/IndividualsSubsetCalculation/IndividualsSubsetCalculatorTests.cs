using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation;
using MCRA.Simulation.Filters.IndividualFilters;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IndividualsSubsetCalculation {

    /// <summary>
    /// Individual subset selection calculator tests.
    /// </summary>
    [TestClass]
    public class IndividualsSubsetCalculatorTests {

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
            var filter = new IndividualSubsetDefinitionFilter(property, subsetDefinition);

            var subset = IndividualsSubsetCalculator.ComputeIndividualsSubset(
                individuals,
                [filter]
            );

            Assert.AreEqual(6, subset.Count);
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

            var subsetDefinition = new IndividualsSubsetDefinition("age", "0-4");
            var filter = new IndividualSubsetDefinitionFilter(property, subsetDefinition);

            var subset = IndividualsSubsetCalculator.ComputeIndividualsSubset(
                individuals,
                [filter]
            );

            Assert.AreEqual(4, subset.Count);
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
            var filter = new IndividualSubsetDefinitionFilter(property, subsetDefinition);

            var subset = IndividualsSubsetCalculator.ComputeIndividualsSubset(
                individuals,
                [filter]
            );

            Assert.AreEqual(4, subset.Count);
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

            var filters = new [] {
                new IndividualSubsetDefinitionFilter(
                    genderProperty,
                    new IndividualsSubsetDefinition(genderProperty.Code, "'F'")
                ),
                new IndividualSubsetDefinitionFilter(
                    ageProperty,
                    new IndividualsSubsetDefinition(ageProperty.Code, "3-6")
                ),
            };

            var subset = IndividualsSubsetCalculator.ComputeIndividualsSubset(
                individuals,
                filters
            );

            Assert.AreEqual(2, subset.Count);
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
    }
}
