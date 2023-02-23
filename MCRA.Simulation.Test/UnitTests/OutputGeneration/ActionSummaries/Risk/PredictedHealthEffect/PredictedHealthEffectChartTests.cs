using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    ///  OutputGeneration, ActionSummaries, Risk, PredictedHealthEffect
    /// </summary>
    [TestClass]
    public class PredictedHealthEffectChartTests : ChartCreatorTestBase {

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
        public void PredictedHealthEffectChart_TestWithoutUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .35, 1.4).ToList();
            var bins = simulateBins(logData);

            var section = new PredictedHealthEffectSection() {
                PHEDistributionBins = bins,
                PercentageZeroIntake = 16.6,
                PercentilesGrid = MockUncertaintyDataPointCollection.Mock(_numberOfSamples, false),
            };

            var chart = new PredictedHealthEffectChartCreator(section, "mg/kg");
            RenderChart(chart, $"TestCreate1");
        }
        /// <summary>
        /// Create chart with uncertainty
        /// </summary>
        [TestMethod]
        public void PredictedHealthEffectChart_TestWithUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .35, 1.4).ToList();
            var bins = simulateBins(logData);

            var section = new PredictedHealthEffectSection() {
                PHEDistributionBins = bins,
                PercentageZeroIntake = 16.6,
                PercentilesGrid = MockUncertaintyDataPointCollection.Mock(_numberOfSamples, true),
            };

            var chart = new PredictedHealthEffectChartCreator(section, "mg/kg");
            RenderChart(chart, $"TestCreate2");
        }
    }
}
