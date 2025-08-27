using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, OIM
    /// </summary>
    [TestClass]
    public class OIMModelSectionTests : SectionTestBase {
        /// <summary>
        /// Test OIMModelSection view
        /// </summary>
        [TestMethod]
        public void OIMModel_Test1() {
            var section = new OIMModelSection();
            AssertIsValidView(section);
        }
    }
}