using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.MarketShares {
    /// <summary>
    /// OutputGeneration, ActionSummaries, MarketShares
    /// </summary>
    [TestClass]
    public class MarketSharesSummarySectionTests : SectionTestBase
    {
        /// <summary>
        /// Test MarketSharesSummarySection view
        /// </summary>
        [TestMethod]
        public void MarketSharesSummarySection_Test() {
            var section = new MarketSharesSummarySection();
            section.Records = new List<MarketShareRecord> {
                new MarketShareRecord()
            };
            AssertIsValidView(section);
        }
    }
}
