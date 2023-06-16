using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, CumulativeThresholdExposureRatio
    /// </summary>
    [TestClass]
    public class IndividualThresholdExposureRatioCumulativeChartTest : ChartCreatorTestBase {

        private int _numberOfSamples = 5000;

        private List<HistogramBin> simulateBins(List<double> data) {
            var weights = Enumerable.Repeat(1D, _numberOfSamples).ToList();
            int numberOfBins = Math.Sqrt(data.Count) < 100 ? BMath.Ceiling(Math.Sqrt(data.Count)) : 100;
            return data.MakeHistogramBins(weights, numberOfBins, data.Min(), data.Max());
        }
        /// <summary>
        /// Create chart without uncertainty
        /// </summary>
        [TestMethod]
        public void IndividualMarginOfExposureCumulativeChart_TestWithoutUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .65, 1.75).ToList();
            var bins = simulateBins(logData);

            var section = new ThresholdExposureRatioDistributionSection() {
                RiskDistributionBins = bins,
                PercentageZeros = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Mock(_numberOfSamples, false),
            };

            var chart = new ThresholdExposureRatioCumulativeChartCreator(section);
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
        /// <summary>
        /// Create chart with uncertainty
        /// </summary>
        [TestMethod]
        public void IndividualThresholdExposureRatioCumulativeChart_TestWithUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .65, 1.75).ToList();
            var bins = simulateBins(logData);

            var section = new ThresholdExposureRatioDistributionSection() {
                RiskDistributionBins = bins,
                PercentageZeros = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Mock(_numberOfSamples, true)
            };

            var chart = new ThresholdExposureRatioCumulativeChartCreator(section);
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
    }
}
