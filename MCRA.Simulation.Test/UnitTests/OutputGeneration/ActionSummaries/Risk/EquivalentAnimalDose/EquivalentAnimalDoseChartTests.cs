using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {
    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, EquivalentAnimalDose
    /// </summary>
    [TestClass]
    public class EquivalentAnimalDoseChartTests : ChartCreatorTestBase {

        private static int _number = 5000;

        private static List<HistogramBin> simulateBins(List<double> data) {
            var weights = Enumerable.Repeat(1D, _number).ToList();
            int numberOfBins = Math.Sqrt(data.Count) < 100 ? BMath.Ceiling(Math.Sqrt(data.Count)) : 100;
            return data.MakeHistogramBins(weights, numberOfBins, data.Min(), data.Max());
        }
        /// <summary>
        /// Create chart without uncertainty 
        /// </summary>
        [TestMethod]
        public void EquivalentAnimalDoseChart_TestWithoutUncertainty() {
            var logData = NormalDistribution.NormalSamples(_number, .52, 1.55).ToList();
            var bins = simulateBins(logData);
            var equivalentAnimalDoseSection = new EquivalentAnimalDoseSection() {
                EADDistributionBins = bins,
                PercentageZeroIntake = 16.6,
                PercentilesGrid = MockUncertaintyDataPointCollection.Mock(_number, false),
            };
            var chart = new EquivalentAnimalDoseChartCreator(equivalentAnimalDoseSection, "mg/kg");
            RenderChart(chart, $"TestEquivalentAnimalDos");
        }
        /// <summary>
        /// Create chart with uncertainty and test view
        /// </summary>
        [TestMethod]
        public void EquivalentAnimalDoseChart_TestWithUncertainty() {
            var logData = NormalDistribution.NormalSamples(_number, .52, 1.55).ToList();
            var bins = simulateBins(logData);
            var equivalentAnimalDoseSection = new EquivalentAnimalDoseSection() {
                EADDistributionBins = bins,
                PercentageZeroIntake = 16.6,
                PercentilesGrid = MockUncertaintyDataPointCollection.Mock(_number, true),
            };
            var chart = new EquivalentAnimalDoseChartCreator(equivalentAnimalDoseSection, "mg/kg");
            RenderChart(chart, $"TestEquivalentAnimalDoseUnc");
        }
    }
}
