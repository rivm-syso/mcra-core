using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.General;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, HazardDistribution
    /// </summary>
    [TestClass]
    public class ExposureHazardRatioPercentileChartCreatorTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart with uncertainty
        /// </summary>
        [TestMethod]
        [DataRow(5.5, 1.1)]
        [DataRow(0.5, 0.1)]
        [DataRow(0.05, 0.01)]
        public void RiskPercentileChart_TestUncertainty(double mu, double sigma) {
            var section = new RiskRatioPercentileSection() {
                Percentiles = mock(100, mu, sigma),
                RiskMetricType = RiskMetricType.ExposureHazardRatio
            };
            var chart = new RiskRatioPercentileChartCreator(section);
            RenderChart(chart, $"TestUncertainty_{mu}_{sigma}");
        }

        private UncertainDataPointCollection<double> mock(int numberOfSamples, double mu, double sigma) {
            var percentages = new List<double>() { 50, 90, 95, 99, 99.5, 99.9 };
            var draw = LogNormalDistribution.LogNormalSamples(numberOfSamples, mu, sigma).ToList();
            var collection = new UncertainDataPointCollection<double>() {
                XValues = percentages,
                ReferenceValues = draw.Percentiles(percentages),
            };
            for (int i = 0; i < 10; ++i) {
                draw = LogNormalDistribution.LogNormalSamples(numberOfSamples, mu, sigma).ToList();
                collection.AddUncertaintyValues(draw.Percentiles(percentages));
            }
            return collection;
        }
    }
}
