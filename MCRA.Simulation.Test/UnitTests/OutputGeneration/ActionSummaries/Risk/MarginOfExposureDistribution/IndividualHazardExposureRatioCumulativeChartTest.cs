using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock;
using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    [TestClass]
    public class IndividualHazardExposureRatioCumulativeChartTest : ChartCreatorTestBase {

        private readonly int _numberOfSamples = 5000;

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

            var section = new RiskRatioDistributionSection() {
                RiskDistributionBins = bins,
                PercentageZeros = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Create(_numberOfSamples, false),
            };

            var chart = new RiskRatioCumulativeChartCreator(section);
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

            var section = new RiskRatioDistributionSection() {
                RiskDistributionBins = bins,
                PercentageZeros = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Create(_numberOfSamples, true),
                UncertaintyLowerLimit = 5,
                UncertaintyUpperLimit = 95
            };

            var chart = new RiskRatioCumulativeChartCreator(section);
            RenderChart(chart, $"TestCreateUnc");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart with uncertainty
        /// </summary>
        [TestMethod]
        public void HazardExposureRatioCumulativeChartCreator_TestUncertaintyLogNormal() {
            var mu = 2;
            var sigma = 1.75;
            var logData = LogNormalDistribution.LogNormalSamples(_numberOfSamples, mu, sigma).Select(c => c > 50 ? 100000 : c).ToList();
            var bins = simulateBins(logData);

            var section = new RiskRatioDistributionSection() {
                RiskDistributionBins = bins,
                PercentageZeros = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Create(_numberOfSamples, mu, sigma, true),
                UncertaintyLowerLimit = 5,
                UncertaintyUpperLimit = 95
            };
            var chart = new RiskRatioCumulativeChartCreator(section);
            RenderChart(chart, $"TestCreateUncLogNormal");
            AssertIsValidView(section);
        }
    }
}
