using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Consumptions {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Consumptions
    /// </summary>
    [TestClass]
    public class FoodAsEatenConsumptionDataSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize and test FoodAsEatenConsumptionDataSection view
        /// </summary>
        [TestMethod]
        public void FoodAsMeasuredConsumptionDataSection_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var individuals = MockIndividualsGenerator.Create(100, 2, random);
            var foods = MockFoodsGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.Create(individuals);
            var consumptions = MockFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var section = new FoodAsEatenConsumptionDataSection();
            section.Summarize(individualDays, foods, consumptions, 25, 75);
            Assert.AreEqual(3, section.Records.Count);
            Assert.AreEqual(23.069, section.Records.First().MeanConsumptionAll, 1e-3);
            AssertIsValidView(section);
        }
    }
}
