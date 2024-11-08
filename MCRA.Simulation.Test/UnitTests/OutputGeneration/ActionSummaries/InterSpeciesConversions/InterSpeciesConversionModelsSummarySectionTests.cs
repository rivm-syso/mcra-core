using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.InterSpeciesConversions {
    /// <summary>
    /// OutputGeneration, ActionSummaries, InterSpeciesConversions
    /// </summary>
    [TestClass]
    public class InterSpeciesConversionModelsSummarySectionTests : SectionTestBase {

        /// <summary>
        /// Test InterSpeciesConversionModelsSummarySection view
        /// </summary>
        [TestMethod]
        public void InterSpeciesConversionModelsSummarySection_Test1() {
            var section = new InterSpeciesConversionModelsSummarySection();
            section.Records = [];
            section.DefaultInterSpeciesFactor = new InterSpeciesConversionModelSummaryRecord();
            AssertIsValidView(section);
        }
    }
}