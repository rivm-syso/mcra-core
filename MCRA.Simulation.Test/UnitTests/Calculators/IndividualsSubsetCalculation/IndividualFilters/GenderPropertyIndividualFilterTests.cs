using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation.IndividualFilters;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IndividualsSubsetCalculation.IndividualFilters {

    /// <summary>
    /// Tests for the gender property individual filter.
    /// </summary>
    [TestClass]
    public class GenderPropertyIndividualFilterTests {

        /// <summary>
        /// Tests gender individual filter. Includes testing of gender aliases.
        /// </summary>
        [TestMethod]
        public void GenderPropertyIndividualFilter_TestPasses() {
            var genderProperty = FakeIndividualPropertiesGenerator.FakeGenderProperty;

            var filter = new GenderPropertyIndividualFilter(genderProperty, ["m"]);

            var propertyValue = new IndividualPropertyValue() {
                IndividualProperty = genderProperty,
            };
            var individual = new Individual(1);
            individual.SetPropertyValue(propertyValue);

            propertyValue.TextValue = "M";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "male";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "F";
            Assert.IsFalse(filter.Passes(individual));
        }

        [TestMethod]
        public void IndividualSubsetDefinitionFilter_TestGender() {
            var property = FakeIndividualPropertiesGenerator.FakeGenderProperty;
            var propertyValue = new IndividualPropertyValue() {
                IndividualProperty = property,
            };
            var individual = new Individual(1);
            individual.SetPropertyValue(propertyValue);

            var filter = new GenderPropertyIndividualFilter(property, ["m"]);

            propertyValue.TextValue = "M";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "male";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "xxx";
            Assert.IsFalse(filter.Passes(individual));

            propertyValue.TextValue = "F";
            Assert.IsFalse(filter.Passes(individual));

            propertyValue.TextValue = "female";
            Assert.IsFalse(filter.Passes(individual));
        }

    }
}
