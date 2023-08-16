using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.CombinedActionSummaries.Risks {
    /// <summary>
    /// Runs the DietaryExposures action
    /// </summary>
    [TestClass]
    public class CombinedRiskPercentilesSectionTests : ChartCreatorTestBase {

        /// <summary>
        /// Test summarizing and rendering of dietary exposure models without uncertainty information.
        /// </summary>
        [TestMethod]
        public void CombinedRiskPercentilesSection_TestNominal() {
            var seed = 1;
            var numModels = new int[] { 1, 3, 6 };
            foreach (var n in numModels) {
                var modelIds = Enumerable.Range(1, n).Select(r => $"Model {r}").ToArray();
                var models = MockRiskModelsGenerator.CreateMockRiskModels(
                    modelIds,
                    new[] { 50, 90, 95, 97.5, 99, 99.9, 99.99 },
                    -1,
                    seed
                );
                var section = new CombinedRiskPercentilesSection();
                section.Summarize(models, RiskMetricType.MarginOfExposure);
                RenderView(section, filename: $"TestNominal_{n}.html");

                var chart = new CombinedRisksChartCreator(section, 99.9);
                RenderChart(chart, $"TestNominal_{n}");

            }
        }

        /// <summary>
        /// Test summarizing and rendering of dietary exposure models with uncertainty information.
        /// </summary>
        [TestMethod]
        public void CombinedRiskPercentilesSection_TestUncertain() {
            var seed = 1;
            var numModels = new int[] { 5 };
            foreach (var n in numModels) {
                var modelIds = Enumerable.Range(1, n).Select(r => $"Model {r}").ToArray();
                //var modelIds = new[] { "NL", "DE", "BE", "GR" };
                var models = MockRiskModelsGenerator.CreateMockRiskModels(
                    modelIds,
                    new[] { 50, 90, 95, 97.5, 99, 99.9, 99.99 },
                    10,
                    seed
                );
                var section = new CombinedRiskPercentilesSection();
                section.Summarize(models, RiskMetricType.MarginOfExposure);
                RenderView(section, filename: $"TestUncertain_{n}.html");

                var chart = new CombinedRisksChartCreator(section, 99.9);
                RenderChart(chart, $"TestUncertain_{n}");
            }
        }
    }
}
