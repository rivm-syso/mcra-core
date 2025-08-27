using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DoseResponseData {

    /// <summary>
    /// OutputGeneration, ActionSummaries, DoseResponseData
    /// </summary>
    [TestClass]
    public class DoseResponseDataSectionTests : SectionTestBase {

        /// <summary>
        /// Test DoseResponseDataSection view
        /// </summary>
        [TestMethod]
        public void DoseResponseDataSection_Test1() {
            var section = new DoseResponseDataSection();
            section.Records = [];
            AssertIsValidView(section);
        }
    }
}
