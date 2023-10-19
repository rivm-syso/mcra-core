using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    [TestClass]
    public class IndividualHazardExposureRatioCumulativeChartTest : ChartCreatorTestBase {

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
        public void HazardExposureRatioCumulativeChartCreator_TestNoUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .65, 1.75).ToList();
            var bins = simulateBins(logData);

            var section = new HazardExposureRatioDistributionSection() {
                RiskDistributionBins = bins,
                PercentageZeros = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Create(_numberOfSamples, false),
            };

            var chart = new HazardExposureRatioCumulativeChartCreator(section);
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart with uncertainty
        /// </summary>
        [TestMethod]
        public void HazardExposureRatioCumulativeChartCreator_TestUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .65, 1.75).ToList();
            var bins = simulateBins(logData);

            var section = new HazardExposureRatioDistributionSection() {
                RiskDistributionBins = bins,
                PercentageZeros = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Create(_numberOfSamples, true)
            };

            var chart = new HazardExposureRatioCumulativeChartCreator(section);
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
    }
}
