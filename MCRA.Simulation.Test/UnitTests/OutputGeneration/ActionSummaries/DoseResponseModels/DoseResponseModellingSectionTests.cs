using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DoseResponseModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DoseResponseModels
    /// </summary>
    [TestClass]
    public class DoseResponseModellingSectionTests : SectionTestBase {

        /// <summary>
        /// Test DoseResponseModellingSection view
        /// </summary>
        [TestMethod]
        public void DoseResponseModellingSection_Test1() {
            var section = new DoseResponseModellingSection() {
                DoseResponseModels = [],
                EffectResponseCombinations = [],
            };
            AssertIsValidView(section);
        }
    }
}