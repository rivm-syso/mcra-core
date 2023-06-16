using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, CumulativeMarginOfExposure
    /// </summary>
    [TestClass]
    public class MarginOfExposureDistributionChartTests : ChartCreatorTestBase {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void IndividualMarginOfExposureChart_TestCreate() {
            var number = 5000;
            var logData = NormalDistribution.NormalSamples(number, .65, 1.75).ToList();
            var bins = simulateBins(logData);

            var section = new ThresholdExposureRatioDistributionSection() {
                RiskDistributionBins = bins,
                PercentageZeros = 10,
            };

            var chart = new ThresholdExposureRatioChartCreator(section);
            RenderChart(chart, "TestCreate");
            AssertIsValidView(section);
        }

        private List<HistogramBin> simulateBins(List<double> data) {
            var weights = Enumerable.Repeat(1D, data.Count).ToList();
            int numberOfBins = Math.Sqrt(data.Count) < 100 ? BMath.Ceiling(Math.Sqrt(data.Count)) : 100;
            return data.MakeHistogramBins(weights, numberOfBins, data.Min(), data.Max());
        }
    }
}