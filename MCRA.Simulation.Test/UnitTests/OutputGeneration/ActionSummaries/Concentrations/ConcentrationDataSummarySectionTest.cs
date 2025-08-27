using MCRA.Simulation.OutputGeneration;

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
