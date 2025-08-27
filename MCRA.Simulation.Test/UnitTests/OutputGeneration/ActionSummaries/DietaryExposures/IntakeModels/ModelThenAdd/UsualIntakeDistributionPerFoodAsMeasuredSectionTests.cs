using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, ModelThenAdd
    /// </summary>
    [TestClass]
    public class UsualIntakeDistributionPerFoodAsMeasuredSectionTests : SectionTestBase {

        /// <summary>
        /// Create chart, test UsualIntakeDistributionPerCategorySection view
        /// </summary>
        [TestMethod]
        public void UsualIntakeDistributionPerFoodAsMeasuredSection_TestView() {
            int seed = 1;
            var categories = MtaFakeDataGenerator.CreateFakeCategories();
            var individualExposuresByCategory = MtaFakeDataGenerator
                .CreateFakeIndividualExposuresByCategory(500, categories, seed);

            var section = new UsualIntakeDistributionPerFoodAsMeasuredSection() {
                IndividualExposuresByCategory = individualExposuresByCategory,
                Categories = categories,
            };

            AssertIsValidView(section);
        }
    }
}