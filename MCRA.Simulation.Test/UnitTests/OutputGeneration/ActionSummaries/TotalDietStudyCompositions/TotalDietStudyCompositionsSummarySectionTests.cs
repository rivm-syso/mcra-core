using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TotalDietStudyCompositions {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TotalDietStudyCompositions
    /// </summary>
    [TestClass]
    public class TotalDietStudyCompositionsSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Summarize and test TotalDietStudyCompositionsSummarySection view
        /// </summary>
        [TestMethod]
        public void TotalDietStudyCompositionsSummarySection_Test1() {
            var foods = FakeFoodsGenerator.Create(2);
            var tDSFoodSampleCompositions = new List<TDSFoodSampleComposition> {
                new() {
                    Food = foods[0],
                    TDSFood = foods[1],
                    Description = "Description",
                    PooledAmount = 1000,
                    Regionality = "Regionality",
                    Seasonality = "Seasonality",
                }
            };
            var section = new TotalDietStudyCompositionsSummarySection();
            section.Summarize(new SectionHeader(), tDSFoodSampleCompositions.ToLookup(c => c.Food));
            Assert.HasCount(1, section.Records);
            AssertIsValidView(section);
        }
    }
}