using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

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
            section.Records = new List<EffectRepresentationRecord>();
            section.Records.Add(new EffectRepresentationRecord() { });
            AssertIsValidView(section);
        }
    }
}