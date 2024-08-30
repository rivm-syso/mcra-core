using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.KineticModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries, KineticModels (AbsorptionFactors)
    /// </summary>
    [TestClass]
    public class AbsorptionFactorsSummarySectionTests : SectionTestBase
    {
        /// <summary>
        /// Test AbsorptionFactorsSummarySection view
        /// </summary>
        [TestMethod]
        public void AbsorptionFactorsSummarySection_Test() {
            var section = new KineticModelsSummarySection();
            AssertIsValidView(section);
        }
    }
}
