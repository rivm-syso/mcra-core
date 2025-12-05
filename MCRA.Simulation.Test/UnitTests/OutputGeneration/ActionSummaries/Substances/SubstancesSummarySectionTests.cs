using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
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
            var substances = FakeSubstancesGenerator.Create(5);
            var section = new SubstancesSummarySection();
            section.Summarize(substances);
            Assert.HasCount(5, section.Records);
            AssertIsValidView(section);
        }
    }
}