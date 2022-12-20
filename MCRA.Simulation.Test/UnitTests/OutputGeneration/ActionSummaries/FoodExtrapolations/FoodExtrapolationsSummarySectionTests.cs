using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.FoodExtrapolations {
    /// <summary>
    /// OutputGeneration, ActionSummaries, FoodExtrapolations
    /// </summary>
    [TestClass]
    public class FoodExtrapolationsSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Test FoodExtrapolationsSummarySection view
        /// </summary>
        [TestMethod]
        public void FoodExtrapolationsSummarySection_Test1() {
            var section = new FoodExtrapolationsSummarySection();
            section.Records = new List<FoodExtrapolationsSummaryRecord>();
            AssertIsValidView(section);
        }
    }
}