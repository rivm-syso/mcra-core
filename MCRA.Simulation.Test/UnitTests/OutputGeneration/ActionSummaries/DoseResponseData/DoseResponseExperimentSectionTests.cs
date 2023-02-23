using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DoseResponseData {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DoseResponseData
    /// </summary>
    [TestClass]
    public class DoseResponseExperimentsSectionTests : SectionTestBase {

        /// <summary>
        /// Test DoseResponseExperimentSection view
        /// </summary>
        [TestMethod]
        public void DoseResponseExperimentsSectionSection_Test1() {
            var section = new DoseResponseExperimentSection();
            section.SubstanceNames = new List<string>() { "A", "B" };
            section.DoseResponseSets = new List<DoseResponseSet>();
            AssertIsValidView(section);
        }
    }
}
