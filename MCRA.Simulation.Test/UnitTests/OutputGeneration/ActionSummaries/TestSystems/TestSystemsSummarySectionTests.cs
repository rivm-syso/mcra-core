using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TestSystems {
    /// <summary>
    /// OutputGeneration, ActionSummaries, TestSystems
    /// </summary>
    [TestClass]
    public class TestSystemsSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Test TestSystemsSummarySection view
        /// </summary>
        [TestMethod]
        public void TestSystemsSummarySection_Test1() {
            var section = new TestSystemsSummarySection();
            Assert.IsNull(section.Records);
            AssertIsValidView(section);
        }
    }
}