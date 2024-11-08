using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    /// OutputGeneration, ActionSummaries,Risk, PredictedHealthEffect
    /// </summary>
    [TestClass]
    public class PredictedHealthEffectCumulativeChartTest : ChartCreatorTestBase {

        private int _numberOfSamples = 5000;

        private List<HistogramBin> simulateBins(List<double> data) {
            var weights = Enumerable.Repeat(1D, _numberOfSamples).ToList();
            int numberOfBins = Math.Sqrt(data.Count) < 100 ? BMath.Ceiling(Math.Sqrt(data.Count)) : 100;
            return data.MakeHistogramBins(weights, numberOfBins, data.Min(), data.Max());
        }
        /// <summary>
        /// Create chart, test PredictedHealthEffectSection view
        /// </summary>
        [TestMethod]
        public void PredictedHealthEffectCumulativeChart_TestWithoutUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .35, 1.4).ToList();
            var bins = simulateBins(logData);
            var percentiles = new UncertainDataPointCollection<double>() {
                XValues = new List<double>() { 50, 95 },
                ReferenceValues = new List<double>() { 1.24, 3.6 },
            };
            percentiles.AddUncertaintyValues(new List<double>() { 1.23, 7 });
            var section = new PredictedHealthEffectSection() {
                PHEDistributionBins = bins,
                PercentageZeroIntake = 16.6,
                PercentilesGrid = MockUncertaintyDataPointCollection.Create(_numberOfSamples, false),
                Percentiles = percentiles,
            };
            var chart = new PredictedHealthEffectCumulativeChartCreator(section, "mg/kg");
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        public void PredictedHealthEffectCumulativeChart_TestWithUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .35, 1.4).ToList();
            var bins = simulateBins(logData);

            var section = new PredictedHealthEffectSection() {
                UncertaintyLowerLimit = 2.5,
                UncertaintyUpperLimit = 97.5,
                PHEDistributionBins = bins,
                PercentageZeroIntake = 16.6,
                PercentilesGrid = MockUncertaintyDataPointCollection.Create(_numberOfSamples, true),
                Percentiles = [],
            };
            var chart = new PredictedHealthEffectCumulativeChartCreator(section, "mg/kg");
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
    }
}
