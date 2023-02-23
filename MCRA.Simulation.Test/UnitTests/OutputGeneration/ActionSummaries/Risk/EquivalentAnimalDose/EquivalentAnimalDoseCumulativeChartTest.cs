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
    public class EquivalentAnimalDoseCumulativeChartTest : ChartCreatorTestBase {

        private static int _numberOfSamples = 5000;

        private List<HistogramBin> simulateBins(List<double> data) {
            var weights = Enumerable.Repeat(1D, _numberOfSamples).ToList();
            int numberOfBins = Math.Sqrt(data.Count) < 100 ? BMath.Ceiling(Math.Sqrt(data.Count)) : 100;
            return data.MakeHistogramBins(weights, numberOfBins, data.Min(), data.Max());
        }
        /// <summary>
        /// Create chart withut uncertainty, test EquivalentAnimalDoseSection view
        /// </summary>
        [TestMethod]
        public void EquivalentAnimalDoseCumulativeChart_TestWithoutUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .55, 1.55).ToList();
            var bins = simulateBins(logData);
            var percentiles = new UncertainDataPointCollection<double>() {
                XValues = new List<double>() { 50, 95},
                ReferenceValues = new List<double>() { 1.24, 3.6 },
            };
            percentiles.AddUncertaintyValues(new List<double>() { 1.23, 7 });
            var section = new EquivalentAnimalDoseSection() {
                EADDistributionBins = bins,
                PercentageZeroIntake = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Mock(_numberOfSamples, false),
                Percentiles = percentiles,
                Reference = new ReferenceDoseRecord(),
            };
            var chart = new EquivalentAnimalDoseCumulativeChartCreator(section, "mg/kg");
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
        /// <summary>
        /// Create chart with uncertainty, test EquivalentAnimalDoseSection view
        /// </summary>
        [TestMethod]
        public void EquivalentAnimalDoseCumulativeChart_TestWithUncertainty() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .55, 1.55).ToList();
            var bins = simulateBins(logData);

            var section = new EquivalentAnimalDoseSection() {
                EADDistributionBins = bins,
                PercentageZeroIntake = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Mock(_numberOfSamples, true),
                Percentiles = new UncertainDataPointCollection<double>(),
                Reference = new ReferenceDoseRecord(),
            };

            var chart = new EquivalentAnimalDoseCumulativeChartCreator(section, "mg/kg");
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
    }
}
