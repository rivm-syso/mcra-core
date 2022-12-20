using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.KineticModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries, KineticModels
    /// </summary>
    [TestClass]
    public class KineticModelsSummarySectionTests : SectionTestBase
    {
        /// <summary>
        /// Test KineticModelsSummarySection view
        /// </summary>
        [TestMethod]
        public void KineticModelsSummarySection_Test() {
            var section = new KineticModelsSummarySection();
            AssertIsValidView(section);
        }
    }
}
