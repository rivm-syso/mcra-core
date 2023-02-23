using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.OccurrencePatterns {
    /// <summary>
    /// OutputGeneration, ActionSummaries, OccurrencePatterns
    /// </summary>
    [TestClass]
    public class OccurrencePatternMixtureSummarySectionTest : SectionTestBase {

        /// <summary>
        /// Test OccurrencePatternMixtureSummarySection view
        /// </summary>
        [TestMethod]
        public void OccurrencePatternMixtureSummarySection_Test1() {
            var section = new OccurrencePatternMixtureSummarySection();
            section.Records = new List<OccurrencePatternMixtureSummaryRecord>();
            section.Records.Add(new OccurrencePatternMixtureSummaryRecord() {
                SubstanceCodes = new List<string> { "A", "B" },
                SubstanceNames = new List<string> { "A", "B" },
            });
            AssertIsValidView(section);
        }
    }
}