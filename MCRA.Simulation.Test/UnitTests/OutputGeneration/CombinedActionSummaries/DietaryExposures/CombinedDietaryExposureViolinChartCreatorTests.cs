using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.CombinedActionSummaries.DietaryExposures {

    /// <summary>
    /// Runs the DietaryExposures action
    /// </summary>
    [TestClass]
    public class CombinedDietaryExposureViolinChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Test summarizing and rendering of dietary exposure models without uncertainty information.
        /// </summary>
        [TestMethod]
        public void CombinedDietaryExposureChartCreatorTests_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var numModels = new int[] { 1, 3, 6 };
            foreach (var n in numModels) {
                var modelIds = Enumerable.Range(1, n).Select(r => $"Model {r}").ToArray();
                var models = FakeDietaryExposureModelsGenerator.CreateMockDietaryExposureModels(
                    modelIds,
                    [50, 90, 95, 97.5, 99, 99.9, 99.99],
                    -1,
                    random
                );
                var section = new CombinedDietaryExposurePercentilesSection();
                section.Summarize(models);
                RenderView(section, filename: $"TestNominal_{n}.html");

                var violinChartCreator = new CombinedDietaryExposuresChartCreator(section, 10);
                RenderChart(violinChartCreator, $"TestNominal_{n}");
            }
        }

        /// <summary>
        /// Test summarizing and rendering of dietary exposure models without uncertainty information.
        /// </summary>
        [TestMethod]
        public void CombinedDietaryExposureViolinChartCreatorTests_Test() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var numModels = new int[] { 3, 30};

            var percentages = new double[] { 50, 90, 99.99 };
            foreach (var n in numModels) {
                var modelIds = Enumerable.Range(1, n).Select(r => $"Model {r}").ToArray();
                var models = FakeDietaryExposureModelsGenerator.CreateMockDietaryExposureModels(
                    modelIds,
                    percentages,
                    10,
                    random
                );
                var section = new CombinedDietaryExposurePercentilesSection();
                section.Summarize(models);
                RenderView(section, filename: $"TestNominal_{n}.html");

                foreach (var percentage in percentages) {
                    var violinChartCreator = new CombinedDietaryExposureViolinChartCreator(section, percentage, true, false, false);
                    RenderChart(violinChartCreator, $"TestNominal_{n}_p{percentage}");
                }
            }
        }
    }
}
