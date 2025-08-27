using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ConsumptionsByModelledFood {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Consumptions
    /// </summary>
    [TestClass]
    public class ModelledFoodsConsumptionInputDataSectionTests : SectionTestBase {
        /// <summary>
        /// Summarize and test ModelledFoodConsumptionDataSection view
        /// </summary>
        [TestMethod]
        public void ModelledFoodsConsumptionDataSection_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var properties = FakeIndividualPropertiesGenerator.Create();
            var population = FakePopulationsGenerator.Create(1);
            var individuals = FakeIndividualsGenerator.Create(100, 2, false, properties, random);
            var foods = FakeFoodsGenerator.Create(3);
            var individualDays = FakeIndividualDaysGenerator.Create(individuals);
            var consumptions = FakeConsumptionsByModelledFoodGenerator.Create(foods, individualDays);
            var section = new ModelledFoodConsumptionDataSection();
            section.Summarize(individualDays, foods, foods, consumptions, 2.5, 97.5);
            Assert.AreEqual(3, section.Records.Count);
            Assert.IsTrue(section.Records.All(r => !double.IsNaN(r.MeanConsumptionAll)));
            AssertIsValidView(section);
        }
    }
}
