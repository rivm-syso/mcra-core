using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.PointsOfDeparture {
    /// <summary>
    /// OutputGeneration, ActionSummaries, PointsOfDeparture
    /// </summary>
    [TestClass]
    public class PointsOfDepartureSummarySectionTests : SectionTestBase {

        /// <summary>
        /// Test PointsOfDepartureSummarySection view
        /// </summary>
        [TestMethod]
        public void PointsOfDepartureSummarySection_Test1() {
            var section = new PointsOfDepartureSummarySection();
            section.Records = [
                new PointsOfDepartureSummaryRecord()
            ];
            AssertIsValidView(section);
        }
    }
}