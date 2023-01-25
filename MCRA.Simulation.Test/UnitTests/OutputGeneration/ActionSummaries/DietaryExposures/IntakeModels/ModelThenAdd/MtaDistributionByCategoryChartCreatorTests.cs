using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {

    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels, ModelThenAdd
    /// </summary>
    [TestClass]
    public class MtaDistributionByCategoryChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create exposure distribution by category chart.
        /// </summary>
        [TestMethod]
        public void MtaDistributionByCategoryChartCreator_TestCreate() {
            int seed = 1;
            var categories = MtaFakeDataGenerator.CreateFakeCategories();
            var individualExposuresByCategory = MtaFakeDataGenerator
                .CreateFakeIndividualExposuresByCategory(500, categories, seed);

            var section = new UsualIntakeDistributionPerCategorySection() {
                IndividualExposuresByCategory = individualExposuresByCategory,
                Categories = categories,
            };

            var chart = new MtaDistributionByCategoryChartCreator(section, "mg/kg bw/day", false);
            TestRender(chart, $"TestCreate", ChartFileType.Png);
        }

        /// <summary>
        /// Create exposure distribution contributions by category chart.
        /// </summary>
        [TestMethod]
        public void MtaDistributionByCategoryChartCreator_TestCreateContributions() {
            int seed = 1;
            var categories = MtaFakeDataGenerator.CreateFakeCategories();
            var individualExposuresByCategory = MtaFakeDataGenerator
                .CreateFakeIndividualExposuresByCategory(500, categories, seed);

            var section = new UsualIntakeDistributionPerCategorySection() {
                IndividualExposuresByCategory = individualExposuresByCategory,
                Categories = categories,
            };

            var chart = new MtaDistributionByCategoryChartCreator(section, "mg/kg bw/day", true);
            RenderChart(chart, $"TestCreateContributions");
        }
    }
}
