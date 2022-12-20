using MCRA.Simulation.Calculators.SampleOriginCalculation;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Concentrations {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Concentrations
    /// </summary>
    [TestClass]
    public class AnalyticalMethodsSummarySectionTests : SectionTestBase
    {
        /// <summary>
        /// Summarize and test AnalyticalMethodsSummarySection view
        /// </summary>
        [TestMethod]
        public void AnalyticalMethodsSummarySection_Test1() {
            var foods = MockFoodsGenerator.Create(3);
            var substances = MockSubstancesGenerator.Create(3);
            var foodSamples = MockSamplesGenerator.CreateFoodSamples(foods, substances, numberOfSamples: 50);

            // Compute sample origins
            var sampleOriginInfos = SampleOriginCalculator.Calculate(foodSamples.ToLookup(c => c.Food, c => c));
            var section = new AnalyticalMethodsSummarySection();
            section.Summarize(foodSamples, substances);
            Assert.AreEqual(5, section.Records.Count);

            AssertIsValidView(section);
        }
    }
}
