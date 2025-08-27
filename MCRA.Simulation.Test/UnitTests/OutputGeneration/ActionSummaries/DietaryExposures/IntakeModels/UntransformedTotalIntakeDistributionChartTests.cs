using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels
    /// </summary>
    [TestClass]
    public class UntransformedTotalIntakeDistributionChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Summarize and test UntransformedTotalIntakeDistributionSection view, create chart
        /// </summary>
        [TestMethod]
        public void UntransformedTotalIntakeDistributionChart_TestCreate() {

            var numIntakes = new[] { 0, 10, 100, 1000 };
            for (int i = 0; i < numIntakes.Length; i++) {
                var number = numIntakes[i];
                var data = NormalDistribution.NormalSamples(number, .5, 1.5).Select(c => Math.Exp(c)).ToList();
                var bins = simulateBins(data, number);
                var section = new UntransformedTotalIntakeDistributionSection() {
                    IntakeDistributionBins = bins,
                    //TotalNumberOfIntakes = number,
                    PercentageZeroIntake = 0
                };

                var chart = new UntransformedTotalIntakeDistributionChartCreator(section, "mg/kg bw/day");
                RenderChart(chart, $"TestCreate{number}");
                AssertIsValidView(section);
            }
        }

        private List<HistogramBin> simulateBins(List<double> data, int number) {
            if (number == 0) {
                return [];
            }
            var weights = Enumerable.Repeat(1D, number).ToList();
            int numberOfBins = Math.Sqrt(data.Count) < 100 ? BMath.Ceiling(Math.Sqrt(data.Count)) : 100;
            return data.MakeHistogramBins(weights, numberOfBins, data.Min(), data.Max());
        }
    }
}