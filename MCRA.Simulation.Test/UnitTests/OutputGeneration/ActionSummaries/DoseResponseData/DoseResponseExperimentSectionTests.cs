using MCRA.Simulation.OutputGeneration;

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
            section.SubstanceNames = ["A", "B"];
            section.DoseResponseSets = [];
            AssertIsValidView(section);
        }
    }
}
