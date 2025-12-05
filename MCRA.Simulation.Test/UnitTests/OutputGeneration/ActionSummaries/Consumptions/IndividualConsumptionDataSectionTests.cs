using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Consumptions {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Consumptions
    /// </summary>
    [TestClass]
    public class IndividualConsumptionDataSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize and test IndividualConsumptionDataSection view
        /// </summary>
        [TestMethod]
        public void IndividualConsumptionDataSection_Test2() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individualDays = FakeIndividualDaysGenerator.Create(20, 2, true, random, properties);
            var consumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var individuals = individualDays.Select(c => c.Individual).ToList();
            var section = new IndividualConsumptionDataSection();

            section.Summarize(new FoodSurvey() { Code = "Test" }, individuals, individualDays, consumptions, null, IndividualSubsetType.IgnorePopulationDefinition, false, null);
            Assert.HasCount(3, section.Records);

            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize and test IndividualConsumptionDataSection view
        /// </summary>
        [TestMethod]
        public void IndividualConsumptionDataSection_Test3() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var individualDays = FakeIndividualDaysGenerator.Create(20, 2, true, random, properties);

            var foodTranslations = FakeFoodTranslationsGenerator.Create(foods, random);
            var foodConsumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var individuals = individualDays.Select(c => c.Individual).ToList();
            var section = new IndividualConsumptionDataSection();
            section.Summarize(new FoodSurvey() { Code = "Test" }, individuals, individualDays, foodConsumptions, consumptionsByModelledFood, IndividualSubsetType.IgnorePopulationDefinition, false, null);
            Assert.HasCount(3, section.Records);
            AssertIsValidView(section);
        }
    }
}
