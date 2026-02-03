using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.KineticConversionFactors {
    /// <summary>
    /// OutputGeneration, ActionSummaries, KineticConversionFactors
    /// </summary>
    [TestClass]
    public class KineticConversionFactorsSummarySectionTests : SectionTestBase
    {
        /// <summary>
        /// Test KineticConversionFactorsSummarySection view
        /// </summary>
        [TestMethod]
        public void KineticConversionFactorsSummarySection_Test() {
            var section = new KineticConversionFactorsDataSummarySection();
            AssertIsValidView(section);
        }
    }
}
