using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, DietaryExposures, IntakeModels, ModelThenAdd
    /// </summary>
    [TestClass]
    public class UsualIntakeDistributionPerCategoryModelSectionTests : SectionTestBase {
        /// <summary>
        /// Test UsualIntakeDistributionPerCategoryModelSection view
        /// </summary>
        [TestMethod]
        public void UsualIntakeDistributionPerCategoryModelSection_TestView() {
            var section = new UsualIntakeDistributionPerCategoryModelSection() {
                IntakeDistributionBins = [],
            };
            AssertIsValidView(section);
        }
    }
}