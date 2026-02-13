using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.CombinedActionSummaries.Risks {
    /// <summary>
    /// Runs the Risks action
    /// </summary>
    [TestClass]
    public class CombinedRiskViolinChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Test summarizing and rendering of risk models with uncertainty.
        /// </summary>
        [TestMethod]
        [DataRow(RiskMetricType.ExposureHazardRatio)]
        public void CombinedRiskViolinChartCreator_TestUncertain(RiskMetricType riskMetric) {
            var seed = 1;
            var numModels = new int[] { 30};
            double[] percentages = riskMetric == RiskMetricType.ExposureHazardRatio
                ? [50, 90, 95, 99, 99.9, 99.99]
                : [0.01, 0.1, 1, 5, 10, 50];
            var scale = 0;
            foreach (var n in numModels) {
                scale++;
                var modelIds = Enumerable.Range(1, n).Select(r => $"Model {r}").ToArray();
                var models = FakeRiskModelsGenerator.CreateMockRiskModels(
                    modelIds,
                    percentages,
                    10,
                    scale,
                    seed
                );
                var section = new CombinedRiskPercentilesSection();

                section.Summarize(models, riskMetric);
                RenderView(section, filename: $"TestUncertain_{n}.html");

                foreach (var percentage in percentages) {
                    var violinChartCreator = new CombinedRisksViolinChartCreator(section, percentage, true, false, false);
                    RenderChart(violinChartCreator, $"TestUncertain_{n}_p{percentage}");
                }
            }
        }
    }
}
