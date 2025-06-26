using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation.IndividualFilters;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.IndividualsSubsetCalculation.IndividualFilters {

    /// <summary>
    /// Tests for the categorical property individual filter.
    /// </summary>
    [TestClass]
    public class CategoricalPropertyIndividualFilterTests {

        /// <summary>
        /// Tests categorical property individual filter.
        /// </summary>
        [TestMethod]
        public void CategoricalPropertyIndividualFilter_TestPasses() {
            var property = FakeIndividualPropertiesGenerator.CreateFake("Cat", IndividualPropertyType.Categorical);

            var propertyValue = new IndividualPropertyValue() {
                IndividualProperty = property,
            };
            var individual = new Individual(1);
            individual.SetPropertyValue(propertyValue);

            var filter = new CategoricalPropertyIndividualFilter(property, ["CatA", "CatB"]);

            propertyValue.TextValue = "cata";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "catb";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "cat";
            Assert.IsFalse(filter.Passes(individual));
        }

        [TestMethod]
        public void IndividualSubsetDefinitionFilter_TestCategorical() {
            var property = FakeIndividualPropertiesGenerator.CreateFake("Cat", IndividualPropertyType.Categorical);
            var propertyValue = new IndividualPropertyValue() {
                IndividualProperty = property,
            };
            var individual = new Individual(1);
            individual.SetPropertyValue(propertyValue);

            var filter = new CategoricalPropertyIndividualFilter(property, ["CatA", "catB"]);

            propertyValue.TextValue = "cata";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "catb";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "cataa";
            Assert.IsFalse(filter.Passes(individual));

            propertyValue.TextValue = "cat";
            Assert.IsFalse(filter.Passes(individual));
        }

    }
}
