using MCRA.Simulation.Calculators.SampleOriginCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Concentrations {

    /// <summary>
    /// OutputGeneration, ActionSummaries, Concentrations
    /// </summary>
    [TestClass]
    public class SampleOriginDataSectionTests : SectionTestBase {

        /// <summary>
        /// Summarize and test SampleOriginDataSection view
        /// </summary>
        [TestMethod]
        public void SampleOriginDataSection_Test1() {
            var foods = FakeFoodsGenerator.Create(3);
            var substances = FakeSubstancesGenerator.Create(3);
            var foodSamples = FakeSamplesGenerator.CreateFoodSamples(foods, substances, numberOfSamples: 50);
            // Compute sample origins
            var sampleOriginInfos = SampleOriginCalculator.Calculate(foodSamples.ToLookup(c => c.Food));
            var section = new SampleOriginsSummarySection();
            section.Summarize(sampleOriginInfos.SelectMany(r => r.Value).ToList());
            Assert.HasCount(9, section.Records);
            AssertIsValidView(section);
        }
    }
}
