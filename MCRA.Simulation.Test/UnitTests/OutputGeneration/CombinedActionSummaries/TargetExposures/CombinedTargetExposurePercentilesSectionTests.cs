using System.Linq;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.Helpers;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.CombinedActionSummaries.TargetExposures {
    /// <summary>
    /// Runs the TargetExposures action
    /// </summary>
    [TestClass]
    public class CombinedTargetExposurePercentilesSectionTests : ChartCreatorTestBase {

        /// <summary>
        /// Test summarizing and rendering of dietary exposure models without uncertainty information.
        /// </summary>
        [TestMethod]
        public void CombinedTargetExposurePercentilesSection_TestNominal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var numModels = new int[] { 1, 3, 6 };
            foreach (var n in numModels) {
                var modelIds = Enumerable.Range(1, n).Select(r => $"Model {r}").ToArray();
                var models = MockTargetExposureModelsGenerator.CreateMockTargetExposureModels(
                    modelIds,
                    new[] { 50, 90, 95, 97.5, 99, 99.9, 99.99 },
                    -1,
                    random
                );
                var section = new CombinedTargetExposurePercentilesSection();
                section.Summarize(models);
                RenderView(section, filename: $"TestNominal_{n}.html");

                var chart = new CombinedTargetExposuresChartCreator(section, 99.9);
                TestRender(chart, $"TestNominal_{n}", ChartFileType.Png);
            }
        }

        /// <summary>
        /// Test summarizing and rendering of dietary exposure models with uncertainty information.
        /// </summary>
        [TestMethod]
        public void CombinedTargetExposurePercentilesSection_TestUncertain() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var numModels = new int[] { 1, 3, 6 };
            foreach (var n in numModels) {
                var modelIds = Enumerable.Range(1, n).Select(r => $"Model {r}").ToArray();
                //var modelIds = new[] { "NL", "DE", "BE", "GR" };
                var models = MockTargetExposureModelsGenerator.CreateMockTargetExposureModels(
                    modelIds,
                    new[] { 50, 90, 95, 97.5, 99, 99.9, 99.99 },
                    20,
                    random
                );
                var section = new CombinedTargetExposurePercentilesSection();
                section.Summarize(models);
                RenderView(section, filename: $"TestUncertain_{n}.html");

                var chart = new CombinedTargetExposuresChartCreator(section, 99.9);
                TestRender(chart, $"TestUncertain_{n}", ChartFileType.Png);
            }
        }
    }
}
