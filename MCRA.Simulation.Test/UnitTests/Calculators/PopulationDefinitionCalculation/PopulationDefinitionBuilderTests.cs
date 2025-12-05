using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.PopulationDefinitionCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.Calculators.PopulationDefinitionCalculation {

    [TestClass]
    public class PopulationDefinitionBuilderTests {

        [TestMethod]
        public void PopulationDefinitionBuilder_TestCreate() {
            var subsetDefinition = new IndividualsSubsetDefinition("age", "2-4");

            var populationBuilder = new PopulationDefinitionBuilder();
            var population = populationBuilder.Create(double.NaN, true, [subsetDefinition], null);

            Assert.HasCount(1, population.PopulationIndividualPropertyValues);

            var agePropertyValue = population.PopulationIndividualPropertyValues["age"];
            Assert.AreEqual(2, agePropertyValue.MinValue);
            Assert.AreEqual(4, agePropertyValue.MaxValue);
        }

        [TestMethod]
        public void IndividualSubsetDefinitionFilter_TestNumericRange_UB() {
            var subsetDefinition = new IndividualsSubsetDefinition("age", "-4");

            var populationBuilder = new PopulationDefinitionBuilder();
            var population = populationBuilder.Create(double.NaN, true, [subsetDefinition], null);

            Assert.HasCount(1, population.PopulationIndividualPropertyValues);

            var agePropertyValue = population.PopulationIndividualPropertyValues["age"];
            Assert.AreEqual(double.NaN, agePropertyValue.MinValue);
            Assert.AreEqual(4, agePropertyValue.MaxValue);
        }

        [TestMethod]
        public void IndividualSubsetDefinitionFilter_TestNumericRange_LB() {
            var subsetDefinition = new IndividualsSubsetDefinition("age", "4-");

            var populationBuilder = new PopulationDefinitionBuilder();
            var population = populationBuilder.Create(double.NaN, true, [subsetDefinition], null);

            Assert.HasCount(1, population.PopulationIndividualPropertyValues);

            var agePropertyValue = population.PopulationIndividualPropertyValues["age"];
            Assert.AreEqual(4, agePropertyValue.MinValue);
            Assert.AreEqual(double.NaN, agePropertyValue.MaxValue);
        }

        [TestMethod]
        public void IndividualSubsetDefinitionFilter_TestCategorical() {
            var subsetDefinition = new IndividualsSubsetDefinition("cat", "'CatA',catB");

            var populationBuilder = new PopulationDefinitionBuilder();
            var population = populationBuilder.Create(double.NaN, true, [subsetDefinition], null);

            Assert.HasCount(1, population.PopulationIndividualPropertyValues);

            var propertyValue = population.PopulationIndividualPropertyValues["cat"];
            CollectionAssert.AreEquivalent(propertyValue.CategoricalLevels.ToArray(), new[] { "CatA", "catB" });
        }

        [TestMethod]
        public void IndividualSubsetDefinitionFilter_TestGender() {
            var property = FakeIndividualPropertiesGenerator.FakeGenderProperty;
            var subsetDefinition = new IndividualsSubsetDefinition(property.Code, "'m'");

            var populationBuilder = new PopulationDefinitionBuilder();
            var population = populationBuilder.Create(double.NaN, true, [subsetDefinition], null);

            Assert.HasCount(1, population.PopulationIndividualPropertyValues);

            var propertyValue = population.PopulationIndividualPropertyValues[property.Code];
            CollectionAssert.AreEquivalent(propertyValue.CategoricalLevels.ToArray(), new[] { "m" });
        }
    }
}
