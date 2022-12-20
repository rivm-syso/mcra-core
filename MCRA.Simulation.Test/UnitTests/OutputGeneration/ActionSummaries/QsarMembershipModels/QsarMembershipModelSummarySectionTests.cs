using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.QsarMembershipModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries, QsarMembershipModels
    /// </summary>
    [TestClass]
    public class QsarMembershipModelsSummarySectionTests : SectionTestBase {

        /// <summary>
        /// Test QsarMembershipModelsSummarySection view
        /// </summary>
        [TestMethod]
        public void QsarMembershipModelsSummarySection_Test1() {
            var section = new QsarMembershipModelsSummarySection();
            Assert.IsNull(section.Records);
            AssertIsValidView(section);
        }
    }
}