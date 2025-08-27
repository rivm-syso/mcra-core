using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

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
            var individuals = FakeIndividualsGenerator.Create(100, 2, random);
            var foods = FakeFoodsGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var consumptions = FakeFoodConsumptionsGenerator.Create(foods, individualDays, random);
            var section = new FoodAsEatenConsumptionDataSection();
            section.Summarize(individualDays, foods, consumptions, 25, 75);
            Assert.AreEqual(3, section.Records.Count);
            Assert.IsTrue(!double.IsNaN(section.Records.First().MeanConsumptionAll));
            AssertIsValidView(section);
        }
    }
}
