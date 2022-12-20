using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.IntraSpeciesFactors {
    /// <summary>
    /// OutputGeneration, ActionSummaries, IntraSpeciesFactors
    /// </summary>
    [TestClass]
    public class IntraSpeciesFactorsSummarySectionTests : SectionTestBase {

        /// <summary>
        /// Test IntraSpeciesFactorsSummarySection view
        /// </summary>
        [TestMethod]
        public void IntraSpeciesFactorsSummarySection_Test1() {
            var section = new IntraSpeciesFactorsSummarySection();
            Assert.IsNull(section.Records);
            AssertIsValidView(section);
        }
    }
}