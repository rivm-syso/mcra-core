using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Filters.IndividualFilters;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Filters.IndividualFilters {

    /// <summary>
    /// Tests for the gender property individual filter.
    /// </summary>
    [TestClass]
    public class BooleanPropertyIndividualFilterTests {

        /// <summary>
        /// Tests gender individual filter. Includes testing of gender aliases.
        /// </summary>
        [TestMethod]
        public void BooleanPropertyIndividualFilter_TestPasses() {
            var booleanProperty = FakeIndividualPropertiesGenerator.FakeBooleanProperty;

            var filter = new BooleanPropertyIndividualFilter(booleanProperty, ["TRUE"]);

            var propertyValue = new IndividualPropertyValue() {
                IndividualProperty = booleanProperty,
            };
            var individual = new Individual(1);
            individual.IndividualPropertyValues.Add(propertyValue);

            propertyValue.TextValue = "T";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "true";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "Yes";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "Y";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "false";
            Assert.IsFalse(filter.Passes(individual));
        }
    }
}
