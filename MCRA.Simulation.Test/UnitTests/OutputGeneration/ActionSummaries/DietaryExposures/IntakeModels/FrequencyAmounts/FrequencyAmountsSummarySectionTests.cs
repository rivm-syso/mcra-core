using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, FrequencyAmounts
    /// </summary>
    [TestClass]
    public class FrequencyAmountsSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Summarize and test FrequencyAmountSummarySection view
        /// </summary>
        [TestMethod]
        public void FrequencyAmountsSummarySection_Test1() {
            int seed = 1;
            var random = new McraRandomGenerator(seed);
            var foods = MockFoodsGenerator.MockFoods("Apple", "Pear", "Bananas");
            var compounds = MockSubstancesGenerator.Create(3);
            var individualDays = MockIndividualDaysGenerator.CreateSimulatedIndividualDays(20, 2, true, random);
            var rpfs = compounds.ToDictionary(r => r, r => 1d);
            var memberships = compounds.ToDictionary(r => r, r => 1d);
            var exposures = MockDietaryIndividualDayIntakeGenerator.Create(individualDays, foods, compounds, 0.5, true, random);

            var section = new FrequencyAmountSummarySection();
            section.Summarize(exposures, rpfs, memberships, false);
            Assert.IsTrue(section.ExposureSummaryRecords.First().Mean > 0);
            Assert.IsTrue(section.FrequencyAmountRelations[0].Median > 0);
            AssertIsValidView(section);
        }
    }
}


