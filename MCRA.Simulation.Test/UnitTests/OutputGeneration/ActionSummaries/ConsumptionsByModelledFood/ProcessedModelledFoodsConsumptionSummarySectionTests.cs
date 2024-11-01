using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ConsumptionsByModelledFood {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Consumptions
    /// </summary>
    [TestClass]
    public class ProcessedModelledFoodsConsumptionSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Summarize and test ProcessedModelledFoodsConsumptionInputDataSection view
        /// </summary>
        [TestMethod]
        public void ProcessedModelledFoodsConsumptionSummarySection_TestsSummarize() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = FakeIndividualsGenerator.Create(100, 2, random);
            var foods = MockFoodsGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var consumptions = MockConsumptionsByModelledFoodGenerator.Create(foods, individualDays);
            var section = new ProcessedModelledFoodConsumptionSummarySection();
            section.Summarize(individualDays, consumptions, 2.5, 97.5);
            Assert.AreEqual(3, section.Records.Count);
            AssertIsValidView(section);
        }
    }
}
