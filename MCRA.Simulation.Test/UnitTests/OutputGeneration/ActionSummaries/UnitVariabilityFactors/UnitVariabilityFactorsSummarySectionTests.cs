using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.UnitVariabilityFactors {
    /// <summary>
    /// OutputGeneration, ActionSummaries, UnitVariabilityFactors
    /// </summary>
    [TestClass]
    public class UnitVariabilityFactorsSummarySectionTests : SectionTestBase {
        /// <summary>
        /// Test UnitVariabilityFactorsSummarySection view
        /// </summary>
        [TestMethod]
        public void UnitVariabilityFactorsSummarySection_Test1() {
            var section = new UnitVariabilityFactorsSummarySection();
            section.Records = new List<UnitVariabilityFactorsRecord>();
            section.Records.Add(new UnitVariabilityFactorsRecord());
            Assert.AreEqual(1, section.Records.Count);
            AssertIsValidView(section);
        }
    }
}