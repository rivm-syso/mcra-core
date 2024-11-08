using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.EffectRepresentations {
    /// <summary>
    /// OutputGeneration, ActionSummaries, EffectRepresentations
    /// </summary>
    [TestClass]
    public class EffectRepresentationsSummarySectionTests : SectionTestBase {

        /// <summary>
        /// Test EffectRepresentationsSummarySection view
        /// </summary>
        [TestMethod]
        public void EffectRepresentationsSummarySection_Test1() {
            var section = new EffectRepresentationsSummarySection();
            section.Records = [
                new EffectRepresentationRecord() { }
            ];
            AssertIsValidView(section);
        }
    }
}