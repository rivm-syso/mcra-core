using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.RelativePotencyFactors {
    /// <summary>
    /// OutputGeneration, ActionSummaries, RelativePotencyFactors
    /// </summary>
    [TestClass]
    public class RelativePotencyFactorsSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Test RelativePotencyFactorsSummarySection view
        /// </summary>
        [TestMethod]
        public void RelativePotencyFactorsSummarySection_Test1() {
            var section = new RelativePotencyFactorsSummarySection();
            section.Records = new List<RelativePotencyFactorsSummaryRecord> {
                new RelativePotencyFactorsSummaryRecord() {
                    RelativePotencyFactorUncertaintyValues = new List<double>()
                }
            };
            AssertIsValidView(section);
        }
    }
}