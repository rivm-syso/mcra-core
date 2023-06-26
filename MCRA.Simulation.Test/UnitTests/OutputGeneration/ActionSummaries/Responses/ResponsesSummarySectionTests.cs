using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Responses {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Responses
    /// </summary>
    [TestClass]
    public class ResponsesSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Test ResponseSummarySection view
        /// </summary>
        [TestMethod]
        public void ResponsesSummarySection_Test1() {
            var section = new ResponseSummarySection();
            section.Records = new List<ResponseSummaryRecord> {
                new ResponseSummaryRecord() { }
            };
            AssertIsValidView(section);
        }
    }
}