using MCRA.Simulation.Constants;
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
            var weights = Enumerable.Repeat(1D, data.Count).ToList();
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
            var mu = 10e5;
            var upper = 15e5;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var percentageZeros = 20d;
            var distribution = LogNormalDistribution.FromMeanAndUpper(mu, upper);
            var logData = distribution.Draws(random, _numberOfSamples);
            var shift = SimulationConstants.MOE_eps - logData.Percentile(75);
            var shifted = new List<double>();
            foreach (var item in logData) {
                shifted.Add(Math.Log10(item + shift));
            }
            var zeros = Enumerable.Repeat(0D, (int)(_numberOfSamples * percentageZeros / 100)).ToList();

            var bins = simulateBins(shifted);
            {
                var section = new RiskRatioDistributionSection() {
                    RiskDistributionBins = bins,
                    PercentageZeros = percentageZeros,
                };
                var chart = new RiskRatioChartCreator(section);
                RenderChart(chart, "TestHistogramLogNormal");
            }
            shifted.AddRange(zeros);
            {
                var section = new RiskRatioDistributionSection() {
                    RiskDistributionBins = bins,
                    PercentageZeros = 0,
                    PercentilesGrid = MockUncertaintyDataPointCollection.Create(
                        _numberOfSamples,
                        mu,
                        upper,
                        shift,
                        percentageZeros,
                        true
                    ),
                    UncertaintyLowerLimit = 5,
                    UncertaintyUpperLimit = 95
                };
                var chart = new RiskRatioCumulativeChartCreator(section);
                RenderChart(chart, $"TestCumulativeLogNormalUnc");
            }

        }
    }
}
