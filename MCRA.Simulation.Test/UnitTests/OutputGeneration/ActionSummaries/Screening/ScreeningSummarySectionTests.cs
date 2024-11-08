using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Screening {
    /// <summary>
    /// OutputGeneration, ActionSummaries,Screening
    /// </summary>
    [TestClass]
    public class ScreeningSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Test ScreeningSummarySection view
        /// </summary>
        [TestMethod]
        public void ScreeningSummarySection_Test1() {
            var section = new ScreeningSummarySection() {
                GroupedScreeningSummaryRecords = [new RestrictedSummaryRecord()],
                ScreeningSummaryRecords = [],
                RiskDriver = new ScreeningSummaryRecord(),
            };
            AssertIsValidView(section);
        }
    }
}