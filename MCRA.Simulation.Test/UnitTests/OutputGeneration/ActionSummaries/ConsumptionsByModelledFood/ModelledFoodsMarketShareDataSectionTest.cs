using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
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
            var foods = FakeFoodsGenerator.Create(5);
            var individualDays = FakeIndividualDaysGenerator.Create(100, 2, false, random);
            var consumptionsByModelledFood = FakeConsumptionsByModelledFoodGenerator.Create(foods, individualDays, true);

            var section = new ModelledFoodMarketShareDataSection();
            section.Summarize(consumptionsByModelledFood);

            AssertIsValidView(section);
        }
    }
}
