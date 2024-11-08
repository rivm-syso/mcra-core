using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Effects {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Effects
    /// </summary>
    [TestClass]
    public class EffectsSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Test EffectsSummarySection view
        /// </summary>
        [TestMethod]
        public void EffectsSummarySection_Test1() {
            var section = new EffectsSummarySection();
            section.Records = [
                new EffectsSummaryRecord() { }
            ];
            AssertIsValidView(section);
        }
    }
}