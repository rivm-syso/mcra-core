using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Filters.IndividualFilters;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Filters.IndividualFilters {

    /// <summary>
    /// Tests for the gender property individual filter.
    /// </summary>
    [TestClass]
    public class NumericPropertyIndividualFilterTests {

        /// <summary>
        /// Tests numeric range individual filter with two lb and ub specified.
        /// </summary>
        [TestMethod]
        public void NumericPropertyIndividualFilter_TestPassesTwoSided() {
            var ageProperty = FakeIndividualPropertiesGenerator.FakeAgeProperty;

            var filter = new NumericPropertyIndividualFilter(ageProperty, 2, 6);

            var propertyValue = new IndividualPropertyValue() {
                IndividualProperty = ageProperty,
            };
            var individual = new Individual(1);
            individual.IndividualPropertyValues.Add(propertyValue);

            propertyValue.DoubleValue = 2;
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.DoubleValue = 6;
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.DoubleValue = 1.9;
            Assert.IsFalse(filter.Passes(individual));

            propertyValue.DoubleValue = 6.1;
            Assert.IsFalse(filter.Passes(individual));

            propertyValue.DoubleValue = null;
            Assert.IsFalse(filter.Passes(individual));

            filter.IncludeMissingValueRecords = true;
            Assert.IsTrue(filter.Passes(individual));
        }

        /// <summary>
        /// Tests numeric range individual filter with only lb specified.
        /// </summary>
        [TestMethod]
        public void NumericPropertyIndividualFilter_TestPassesLowerBound() {
            var ageProperty = FakeIndividualPropertiesGenerator.FakeAgeProperty;

            var filter = new NumericPropertyIndividualFilter(ageProperty, 2, null);

            var propertyValue = new IndividualPropertyValue() {
                IndividualProperty = ageProperty,
            };
            var individual = new Individual(1);
            individual.IndividualPropertyValues.Add(propertyValue);

            propertyValue.DoubleValue = 2;
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.DoubleValue = 6;
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.DoubleValue = 1.9;
            Assert.IsFalse(filter.Passes(individual));

            propertyValue.DoubleValue = 6.1;
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.DoubleValue = null;
            Assert.IsFalse(filter.Passes(individual));

            filter.IncludeMissingValueRecords = true;
            Assert.IsTrue(filter.Passes(individual));
        }

        /// <summary>
        /// Tests numeric range individual filter with only ub specified.
        /// </summary>
        [TestMethod]
        public void NumericPropertyIndividualFilter_TestPassesUpperBound() {
            var ageProperty = FakeIndividualPropertiesGenerator.FakeAgeProperty;

            var filter = new NumericPropertyIndividualFilter(ageProperty, null, 6);

            var propertyValue = new IndividualPropertyValue() {
                IndividualProperty = ageProperty,
            };
            var individual = new Individual(1);
            individual.IndividualPropertyValues.Add(propertyValue);

            propertyValue.DoubleValue = 2;
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.DoubleValue = 6;
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.DoubleValue = 1.9;
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.DoubleValue = 6.1;
            Assert.IsFalse(filter.Passes(individual));

            propertyValue.DoubleValue = null;
            Assert.IsFalse(filter.Passes(individual));

            filter.IncludeMissingValueRecords = true;
            Assert.IsTrue(filter.Passes(individual));
        }
    }
}
