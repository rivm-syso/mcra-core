using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ConsumptionsByModelledFood {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Consumptions
    /// </summary>
    [TestClass]
    public class ModelledFoodsMarketShareDataSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize, test ModelledFoodsMarketShareDataSection view
        /// </summary>
        [TestMethod]
        public void ModelledFoodsMarketShareDataSection_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.Create(5);
            var individualDays = MockIndividualDaysGenerator.Create(100, 2, false, random);
            var consumptionsByModelledFood = MockConsumptionsByModelledFoodGenerator.Create(foods, individualDays, true);

            var section = new ModelledFoodMarketShareDataSection();
            section.Summarize(consumptionsByModelledFood);

            AssertIsValidView(section);
        }
    }
}
