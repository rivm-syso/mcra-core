using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.General;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, HazardDistribution
    /// </summary>
    [TestClass]
    public class HazardDistributionCumulativeChartTest : ChartCreatorTestBase {

        private readonly int _numberOfSamples = 5000;

        private List<HistogramBin> simulateBins() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .5, 1.5).ToList();
            var weights = Enumerable.Repeat(1D, _numberOfSamples).ToList();
            int numberOfBins = Math.Sqrt(logData.Count) < 100 ? BMath.Ceiling(Math.Sqrt(logData.Count)) : 100;
            return logData.MakeHistogramBins(weights, numberOfBins, logData.Min(), logData.Max());
        }

        /// <summary>
        /// Create chart and test view for nominal run.
        /// </summary>
        [TestMethod]
        public void HazardDistributionCumulativeChart_TestNominal() {
            var section = new HazardDistributionSection() {
                CEDDistributionBins = simulateBins(),
                PercentageZeroIntake = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Create(_numberOfSamples, false),
                TargetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay)
            };
            var chart = new HazardDistributionCumulativeChartCreator(section, "mg/kg");
            RenderChart(chart, $"TestNominal");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart and test view for uncertainty results.
        /// </summary>
        [TestMethod]
        public void HazardDistributionCumulativeChart_TestUncertainty() {
            var section = new HazardDistributionSection() {
                CEDDistributionBins = simulateBins(),
                PercentageZeroIntake = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Create(_numberOfSamples, true),
                TargetUnit = TargetUnit.FromExternalExposureUnit(ExternalExposureUnit.ugPerKgBWPerDay)
            };
            var chart = new HazardDistributionCumulativeChartCreator(section, "mg/kg");
            RenderChart(chart, $"TestUncertainty");
            AssertIsValidView(section);
        }
    }
}
