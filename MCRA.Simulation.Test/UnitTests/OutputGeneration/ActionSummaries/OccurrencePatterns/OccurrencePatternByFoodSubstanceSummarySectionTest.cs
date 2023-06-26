using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.OccurrencePatterns {
    /// <summary>
    /// OutputGeneration, ActionSummaries, OccurrencePatterns
    /// </summary>
    [TestClass]
    public class OccurrencePatternByFoodSubstanceSummarySectionTest : SectionTestBase {

        /// <summary>
        /// Test AgriculturalUseByFoodSubstanceSummarySection view
        /// </summary>
        [TestMethod]
        public void OccurrencePatternByFoodSubstanceSummarySection_TestSummarize() {
            var section = new OccurrenceFrequenciesSummarySection();
            section.Records = new List<AgriculturalUseByFoodSubstanceSummaryRecord> {
                new AgriculturalUseByFoodSubstanceSummaryRecord() {
                    AgriculturalUseFractionUncertaintyValues = new List<double>() { .1, .2, .3, .4, .5 }
                }
            };
            AssertIsValidView(section);
        }
    }
}