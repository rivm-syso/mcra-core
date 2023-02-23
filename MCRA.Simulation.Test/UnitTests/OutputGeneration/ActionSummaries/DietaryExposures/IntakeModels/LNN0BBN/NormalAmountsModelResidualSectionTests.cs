using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, LNN0BBN
    /// </summary>
    [TestClass]
    public class NormalAmountsModelResidualSectionTests : SectionTestBase {
        /// <summary>
        /// Test NormalAmountsModelResidualSection view 
        /// </summary>
        [TestMethod]
        public void NormalAmountsModelResidualSection_Test1() {
            var section = new NormalAmountsModelResidualSection() {
                Residuals = new List<double>() { 0, 1, 2, 3, 4, 5 },
            };
            AssertIsValidView(section);
        }
    }
}


