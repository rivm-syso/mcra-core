using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DoseResponseModels {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DoseResponseModels
    /// </summary>
    [TestClass]
    public class DoseResponseModelsDetailsSectionTests : SectionTestBase {

        /// <summary>
        /// Test DoseResponseModelsDetailsSection view
        /// </summary>
        [TestMethod]
        public void DoseResponseModelDetailsSection_Test1() {
            var section = new DoseResponseModelsDetailsSection();
            section.DoseResponseModels = [];
            AssertIsValidView(section);
        }
    }
}