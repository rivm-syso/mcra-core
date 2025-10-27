using MCRA.General;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.OutputGeneration.CombinedActionSummaries.Risks;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.CombinedActionSummaries.Risks {
    /// <summary>
    /// Runs the DietaryExposures action
    /// </summary>
    [TestClass]
    public class CombinedRiskPercentilesSectionTests : ChartCreatorTestBase {

        /// <summary>
        /// Test summarizing and rendering of risk models without uncertainty.
        /// </summary>
        [TestMethod]
        [DataRow(RiskMetricType.ExposureHazardRatio)]
        [DataRow(RiskMetricType.HazardExposureRatio)]
        public void CombinedRiskPercentilesSection_TestNominal(RiskMetricType riskMetric) {
            var seed = 1;
            var numModels = new int[] { 1, 3, 6 };
            double[] percentages = riskMetric == RiskMetricType.ExposureHazardRatio
                ? [50, 90, 95, 99, 99.9, 99.99]
                : [0.01, 0.1, 1, 5, 10, 50];
            foreach (var n in numModels) {
                var modelIds = Enumerable.Range(1, n).Select(r => $"Model {r}").ToArray();
                var models = FakeRiskModelsGenerator.CreateMockRiskModels(
                    modelIds,
                    percentages,
                    -1,
                    seed
                );
                var section = new CombinedRiskPercentilesSection();
                section.Summarize(models, riskMetric);
                RenderView(section, filename: $"TestNominal_{n}.html");

                var chart = new CombinedRisksSafetyChartCreator(section);
                RenderChart(chart, $"TestNominal_{n}");
            }
        }

        /// <summary>
        /// Test summarizing and rendering of risk models with uncertainty.
        /// </summary>
        [TestMethod]
        [DataRow(RiskMetricType.ExposureHazardRatio)]
        [DataRow(RiskMetricType.HazardExposureRatio)]
        public void CombinedRiskPercentilesSection_TestUncertain(RiskMetricType riskMetric) {
            var seed = 1;
            var numModels = new int[] { 5 };
            double[] percentages = riskMetric == RiskMetricType.ExposureHazardRatio
                ? [50, 90, 95, 99, 99.9, 99.99]
                : [0.01, 0.1, 1, 5, 10, 50];
            foreach (var n in numModels) {
                var modelIds = Enumerable.Range(1, n).Select(r => $"Model {r}").ToArray();
                var models = FakeRiskModelsGenerator.CreateMockRiskModels(
                    modelIds,
                    percentages,
                    10,
                    seed
                );
                var section = new CombinedRiskPercentilesSection();
                section.Summarize(models, riskMetric);
                RenderView(section, filename: $"TestUncertain_{n}.html");

                var chart = new CombinedRisksSafetyChartCreator(section);
                RenderChart(chart, $"TestUncertain_{n}");
            }
        }
    }
}
