using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Concentrations {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Concentrations
    /// </summary>
    [TestClass]
    public class ConcentrationDataSummarySectionTests : SectionTestBase
    {
        /// <summary>
        /// Test ConcentrationDataSummarySection view
        /// </summary>
        [TestMethod]
        public void ConcentrationDataSummarySection_Test() {
            var section = new ConcentrationDataSummarySection();
            AssertIsValidView(section);
        }
    }
}
