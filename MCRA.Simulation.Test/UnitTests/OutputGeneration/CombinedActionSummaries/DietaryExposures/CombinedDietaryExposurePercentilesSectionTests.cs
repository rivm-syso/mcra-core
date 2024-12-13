using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.CombinedActionSummaries.DietaryExposures {

    /// <summary>
    /// Runs the DietaryExposures action
    /// </summary>
    [TestClass]
    public class CombinedDietaryExposurePercentilesSectionTests : ChartCreatorTestBase {

        /// <summary>
        /// Test summarizing and rendering of dietary exposure models without uncertainty information.
        /// </summary>
        [TestMethod]
        public void CombinedDietaryExposurePercentilesSection_TestNominal() {
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

                var chart = new CombinedDietaryExposuresChartCreator(section, 99.9);
                RenderChart(chart, $"TestNominal_{n}");
            }
        }

        /// <summary>
        /// Test summarizing and rendering of dietary exposure models with uncertainty information.
        /// </summary>
        [TestMethod]
        public void CombinedDietaryExposurePercentilesSection_TestUncertain() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var numModels = new int[] { 1, 3, 6 };
            foreach (var n in numModels) {
                var modelIds = Enumerable.Range(1, n).Select(r => $"Model {r}").ToArray();
                //var modelIds = new[] { "NL", "DE", "BE", "GR" };
                var models = FakeDietaryExposureModelsGenerator.CreateMockDietaryExposureModels(
                    modelIds,
                    [50, 90, 95, 97.5, 99, 99.9, 99.99],
                    10,
                    random
                );
                var section = new CombinedDietaryExposurePercentilesSection();
                section.Summarize(models);
                RenderView(section, filename: $"TestUncertain_{n}.html");

                var chart = new CombinedDietaryExposuresChartCreator(section, 99.9);
                RenderChart(chart, $"TestUncertain_{n}");
            }
        }
    }
}
