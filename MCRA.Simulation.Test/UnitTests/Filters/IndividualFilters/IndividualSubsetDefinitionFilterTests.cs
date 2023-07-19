using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Filters.IndividualFilters;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Filters.IndividualFilters {

    /// <summary>
    /// Tests for the gender property individual filter.
    /// </summary>
    [TestClass]
    public class IndividualSubsetDefinitionFilterTests {

        [TestMethod]
        public void IndividualSubsetDefinitionFilter_TestNumericRange() {
            var property = MockIndividualPropertiesGenerator.FakeAgeProperty;

            var propertyValue = new IndividualPropertyValue() {
                IndividualProperty = property,
            };
            var individual = new Individual(1);
            individual.IndividualPropertyValues.Add(propertyValue);

            var subsetDefinition = new IndividualsSubsetDefinitionDto("age", "2-4");
            var filter = new IndividualSubsetDefinitionFilter(property, subsetDefinition);

            propertyValue.DoubleValue = 1;
            Assert.IsFalse(filter.Passes(individual));

            propertyValue.DoubleValue = 2;
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.DoubleValue = 4;
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.DoubleValue = 5;
            Assert.IsFalse(filter.Passes(individual));
        }

        [TestMethod]
        public void IndividualSubsetDefinitionFilter_TestNumericRange_UB() {
            var property = MockIndividualPropertiesGenerator.FakeAgeProperty;

            var propertyValue = new IndividualPropertyValue() {
                IndividualProperty = property,
            };
            var individual = new Individual(1);
            individual.IndividualPropertyValues.Add(propertyValue);

            var subsetDefinition = new IndividualsSubsetDefinitionDto("age", "-4");
            var filter = new IndividualSubsetDefinitionFilter(property, subsetDefinition);

            propertyValue.DoubleValue = 3;
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.DoubleValue = 4;
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.DoubleValue = 5;
            Assert.IsFalse(filter.Passes(individual));
        }

        [TestMethod]
        public void IndividualSubsetDefinitionFilter_TestNumericRange_LB() {
            var property = MockIndividualPropertiesGenerator.FakeAgeProperty;

            var propertyValue = new IndividualPropertyValue() {
                IndividualProperty = property,
            };
            var individual = new Individual(1);
            individual.IndividualPropertyValues.Add(propertyValue);

            var subsetDefinition = new IndividualsSubsetDefinitionDto("age", "4-");
            var filter = new IndividualSubsetDefinitionFilter(property, subsetDefinition);

            propertyValue.DoubleValue = 3;
            Assert.IsFalse(filter.Passes(individual));

            propertyValue.DoubleValue = 4;
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.DoubleValue = 5;
            Assert.IsTrue(filter.Passes(individual));
        }

        [TestMethod]
        public void IndividualSubsetDefinitionFilter_TestCategorical() {
            var property = MockIndividualPropertiesGenerator.CreateFake("Cat", IndividualPropertyType.Categorical);
            var propertyValue = new IndividualPropertyValue() {
                IndividualProperty = property,
            };
            var individual = new Individual(1);
            individual.IndividualPropertyValues.Add(propertyValue);

            var subsetDefinition = new IndividualsSubsetDefinitionDto("cat", "'CatA',catB");
            var filter = new IndividualSubsetDefinitionFilter(property, subsetDefinition);

            propertyValue.TextValue = "cata";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "catb";
            Assert.IsTrue(filter.Passes(individual));

            propertyValue.TextValue = "cataa";
            Assert.IsFalse(filter.Passes(individual));

            propertyValue.TextValue = "cat";
            Assert.IsFalse(filter.Passes(individual));
        }

        [TestMethod]
        public void IndividualSubsetDefinitionFilter_TestGender() {
            var property = MockIndividualPropertiesGenerator.FakeGenderProperty;
            var propertyValue = new IndividualPropertyValue() {
                IndividualProperty = property,
            };
            var individual = new Individual(1);
            individual.IndividualPropertyValues.Add(propertyValue);

            var subsetDefinition = new IndividualsSubsetDefinitionDto(property.Code, "'m'");
            var filter = new IndividualSubsetDefinitionFilter(property, subsetDefinition);

            propertyValue.TextValue = "M";
            Assert.IsTrue(filter.Passes(individual));

            // Note: the subset definition filter does not account for gender types;
            // if such functionality is desired, a specific type property individual
            // filter should be used (e.g., gender type property individual filter).
            propertyValue.TextValue = "male";
            Assert.IsFalse(filter.Passes(individual));

            propertyValue.TextValue = "F";
            Assert.IsFalse(filter.Passes(individual));

            propertyValue.TextValue = "female";
            Assert.IsFalse(filter.Passes(individual));
        }
    }
}
