using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.ConcentrationLimits {
    /// <summary>
    /// OutputGeneration, ActionSummaries, ConcentrationLimits
    /// </summary>
    [TestClass]
    public class ConcentrationLimitsSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Test ConcentrationLimitsDataSection view
        /// </summary>
        [TestMethod]
        public void ConcentrationLimitsSummarySection_Test1() {
            var section = new ConcentrationLimitsDataSection();
            section.Records = [
                new ConcentrationLimitsDataRecord() { }
            ];
            AssertIsValidView(section);
        }
    }
}