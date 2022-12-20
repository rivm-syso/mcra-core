using MCRA.Utils.Statistics;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

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
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.Create(20, 2, true, random, properties);
            var consumptions = MockFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var individuals = individualDays.Select(c => c.Individual).ToList();
            var section = new IndividualConsumptionDataSection();

            section.Summarize(new FoodSurvey() { Code = "Test" }, individuals, individualDays, consumptions, null, IndividualSubsetType.IgnorePopulationDefinition, false, null);
            Assert.AreEqual(3, section.Records.Count);

            AssertIsValidView(section);
        }
        /// <summary>
        /// Summarize and test IndividualConsumptionDataSection view
        /// </summary>
        [TestMethod]
        public void IndividualConsumptionDataSection_Test3() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var properties = MockIndividualPropertiesGenerator.Create();
            var individualDays = MockIndividualDaysGenerator.Create(20, 2, true, random, properties);

            var foodTranslations = MockFoodTranslationsGenerator.Create(foods, random);
            var foodConsumptions = MockFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator
                .Create(
                    foodConsumptions,
                    foodTranslations,
                    substances
                );

            var individuals = individualDays.Select(c => c.Individual).ToList();
            var section = new IndividualConsumptionDataSection();
            section.Summarize(new FoodSurvey() { Code = "Test" }, individuals, individualDays, foodConsumptions, consumptionsByModelledFood, IndividualSubsetType.IgnorePopulationDefinition, false, null);
            Assert.AreEqual(3, section.Records.Count);
            AssertIsValidView(section);
        }
    }
}
