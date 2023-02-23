using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, HazardDistribution
    /// </summary>
    [TestClass]
    public class HazardDistributionCumulativeChartTest : ChartCreatorTestBase {

        private int _numberOfSamples = 5000;

        private List<HistogramBin> simulateBins(List<double> data) {
            var weights = Enumerable.Repeat(1D, _numberOfSamples).ToList();
            int numberOfBins = Math.Sqrt(data.Count) < 100 ? BMath.Ceiling(Math.Sqrt(data.Count)) : 100;
            return data.MakeHistogramBins(weights, numberOfBins, data.Min(), data.Max());
        }
        /// <summary>
        /// Create chart and test HazardDistributionSection view, without uncertainty
        /// </summary>
        [TestMethod]
        public void HazardDistributionCumulativeChart_TestWithoutUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .5, 1.5).ToList();
            var referenceDose = Math.Pow(10, logData.Percentile(82));
            var bins = simulateBins(logData);

            var section = new HazardDistributionSection() {
                CEDDistributionBins = bins,
                PercentageZeroIntake = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Mock(_numberOfSamples, false),
                Reference = new ReferenceDoseRecord(),
            };
            var chart = new HazardDistributionCumulativeChartCreator(section, "mg/kg");
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
        /// <summary>
        /// Create chart, with uncertainty
        /// </summary>
        [TestMethod]
        public void HazardDistributionCumulativeChart_TestWithUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .5, 1.5).ToList();
            var referenceDose = Math.Pow(10, logData.Percentile(82));
            var bins = simulateBins(logData);

            var section = new HazardDistributionSection() {
                CEDDistributionBins = bins,
                PercentageZeroIntake = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Mock(_numberOfSamples, true)
            };

            var chart = new HazardDistributionCumulativeChartCreator(section, "mg/kg");
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
    }
}