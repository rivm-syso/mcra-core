using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Foods {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Foods
    /// </summary>
    [TestClass]
    public class FoodsSummarySectionTests : SectionTestBase {

        /// <summary>
        /// Test FoodsSummarySection view
        /// </summary>
        [TestMethod]
        public void FoodsSummarySection_TestHasValidView() {
            var section = new FoodsSummarySection {
                Records = new List<FoodsSummaryRecord>()
            };
            section.Records.Add(new FoodsSummaryRecord() { });
            AssertIsValidView(section);
        }
    }
}