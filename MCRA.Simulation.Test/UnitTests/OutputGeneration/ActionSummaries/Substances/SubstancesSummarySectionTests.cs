using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Substances {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Substances
    /// </summary>

    [TestClass]
    public class SubstancesSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Summarize and test SubstancesSummarySection view
        /// </summary>
        [TestMethod]
        public void SubstancesSummarySection_Test1() {
            var substances = MockSubstancesGenerator.Create(5);
            var section = new SubstancesSummarySection();
            section.Summarize(substances);
            Assert.AreEqual(5, section.Records.Count);
            AssertIsValidView(section);
        }
    }
}