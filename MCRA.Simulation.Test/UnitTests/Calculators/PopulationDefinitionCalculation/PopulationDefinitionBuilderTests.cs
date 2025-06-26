using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.IndividualsSubsetCalculation.IndividualFilters;
using MCRA.Simulation.Calculators.PopulationDefinitionCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.PopulationDefinitionCalculation {

    [TestClass]
    public class PopulationDefinitionBuilderTests {

        [TestMethod]
        public void PopulationDefinitionBuilder_TestCreate() {
            var subsetDefinition = new IndividualsSubsetDefinition("age", "2-4");

            var populationBuilder = new PopulationDefinitionBuilder();
            var population = populationBuilder.Create(double.NaN, true, [subsetDefinition], null);

            Assert.AreEqual(population.PopulationIndividualPropertyValues.Count, 1);

            var agePropertyValue = population.PopulationIndividualPropertyValues["age"];
            Assert.AreEqual(agePropertyValue.MinValue, 2);
            Assert.AreEqual(agePropertyValue.MaxValue, 4);
        }

        [TestMethod]
        public void IndividualSubsetDefinitionFilter_TestNumericRange_UB() {
            var subsetDefinition = new IndividualsSubsetDefinition("age", "-4");

            var populationBuilder = new PopulationDefinitionBuilder();
            var population = populationBuilder.Create(double.NaN, true, [subsetDefinition], null);

            Assert.AreEqual(population.PopulationIndividualPropertyValues.Count, 1);

            var agePropertyValue = population.PopulationIndividualPropertyValues["age"];
            Assert.AreEqual(agePropertyValue.MinValue, double.NaN);
            Assert.AreEqual(agePropertyValue.MaxValue, 4);
        }

        [TestMethod]
        public void IndividualSubsetDefinitionFilter_TestNumericRange_LB() {
            var subsetDefinition = new IndividualsSubsetDefinition("age", "4-");

            var populationBuilder = new PopulationDefinitionBuilder();
            var population = populationBuilder.Create(double.NaN, true, [subsetDefinition], null);

            Assert.AreEqual(population.PopulationIndividualPropertyValues.Count, 1);

            var agePropertyValue = population.PopulationIndividualPropertyValues["age"];
            Assert.AreEqual(agePropertyValue.MinValue, 4);
            Assert.AreEqual(agePropertyValue.MaxValue, double.NaN);
        }

        [TestMethod]
        public void IndividualSubsetDefinitionFilter_TestCategorical() {
            var subsetDefinition = new IndividualsSubsetDefinition("cat", "'CatA',catB");

            var populationBuilder = new PopulationDefinitionBuilder();
            var population = populationBuilder.Create(double.NaN, true, [subsetDefinition], null);

            Assert.AreEqual(population.PopulationIndividualPropertyValues.Count, 1);

            var propertyValue = population.PopulationIndividualPropertyValues["cat"];
            CollectionAssert.AreEquivalent(propertyValue.CategoricalLevels.ToArray(), new[] { "CatA", "catB" });
        }

        [TestMethod]
        public void IndividualSubsetDefinitionFilter_TestGender() {
            var property = FakeIndividualPropertiesGenerator.FakeGenderProperty;
            var subsetDefinition = new IndividualsSubsetDefinition(property.Code, "'m'");

            var populationBuilder = new PopulationDefinitionBuilder();
            var population = populationBuilder.Create(double.NaN, true, [subsetDefinition], null);

            Assert.AreEqual(population.PopulationIndividualPropertyValues.Count, 1);

            var propertyValue = population.PopulationIndividualPropertyValues[property.Code];
            CollectionAssert.AreEquivalent(propertyValue.CategoricalLevels.ToArray(), new[] { "m" });
        }
    }
}
