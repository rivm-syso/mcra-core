using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.Risk {

    /// <summary>
    /// OutputGeneration, ActionSummaries, Risk, EquivalentAnimalDose
    /// </summary>
    [TestClass]
    public class EquivalentAnimalDoseCumulativeChartTest : ChartCreatorTestBase {

        private static readonly int _numberOfSamples = 5000;

        private static List<HistogramBin> simulateBins() {
            var logData = NormalDistribution.NormalSamples(_numberOfSamples, .55, 1.55).ToList();
            var weights = Enumerable.Repeat(1D, _numberOfSamples).ToList();
            int numberOfBins = Math.Sqrt(logData.Count) < 100 ? BMath.Ceiling(Math.Sqrt(logData.Count)) : 100;
            return logData.MakeHistogramBins(weights, numberOfBins, logData.Min(), logData.Max());
        }

        /// <summary>
        /// Create chart and render view for nominal results record.
        /// </summary>
        [TestMethod]
        public void EquivalentAnimalDoseCumulativeChart_TestWithoutUncertainty() {
            var percentiles = new UncertainDataPointCollection<double>() {
                XValues = new List<double>() { 50, 95 },
                ReferenceValues = new List<double>() { 1.24, 3.6 },
            };
            percentiles.AddUncertaintyValues(new List<double>() { 1.23, 7 });
            var section = new EquivalentAnimalDoseSection() {
                EADDistributionBins = simulateBins(),
                PercentageZeroIntake = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Create(_numberOfSamples, false),
                Percentiles = percentiles,
                Reference = new ReferenceDoseRecord(new Compound("X")),
            };
            var chart = new EquivalentAnimalDoseCumulativeChartCreator(section, "mg/kg");
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Create chart and render view with uncertainty.
        /// </summary>
        [TestMethod]
        public void EquivalentAnimalDoseCumulativeChart_TestWithUncertainty() {
            var section = new EquivalentAnimalDoseSection() {
                EADDistributionBins = simulateBins(),
                PercentageZeroIntake = 0,
                PercentilesGrid = MockUncertaintyDataPointCollection.Create(_numberOfSamples, true),
                Percentiles = [],
                Reference = new ReferenceDoseRecord(new Compound("X")),
            };

            var chart = new EquivalentAnimalDoseCumulativeChartCreator(section, "mg/kg");
            RenderChart(chart, $"TestCreate");
            AssertIsValidView(section);
        }
    }
}
