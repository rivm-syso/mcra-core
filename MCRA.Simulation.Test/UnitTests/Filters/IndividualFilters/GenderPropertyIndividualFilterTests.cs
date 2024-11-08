using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Filters.IndividualFilters;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Filters.IndividualFilters {

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

            var filter = new GenderPropertyIndividualFilter(genderProperty, new[] { "m" });

            var propertyValue = new IndividualPropertyValue() {
                IndividualProperty = genderProperty,
            };
            var individual = new Individual(1);
            individual.IndividualPropertyValues.Add(propertyValue);

            propertyValue.TextValue = "M";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "male";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "F";
            Assert.IsFalse(filter.Passes(individual));
        }
    }
}
