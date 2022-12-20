using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, ModelThenAdd
    /// </summary>
    [TestClass]
    public class UsualIntakeDistributionPerCategorySectionTests : SectionTestBase {

        /// <summary>
        /// Create chart, test UsualIntakeDistributionPerCategorySection view
        /// </summary>
        [TestMethod]
        public void UsualIntakeDistributionPerCategorySection_TestView() {
            int seed = 1;
            var categories = MtaFakeDataGenerator.CreateFakeCategories();
            var individualExposuresByCategory = MtaFakeDataGenerator
                .CreateFakeIndividualExposuresByCategory(500, categories, seed);

            var section = new UsualIntakeDistributionPerCategorySection() {
                IndividualExposuresByCategory = individualExposuresByCategory,
                Categories = categories,
            };

            AssertIsValidView(section);
        }
    }
}
