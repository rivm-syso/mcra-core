using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.DietaryExposures {
    /// <summary>
    /// OutputGeneration, ActionSummaries, DietaryExposures, IntakeModels
    /// </summary>
    [TestClass]
    public class OIMChartCreatorChartTests : ChartCreatorTestBase {
        /// <summary>
        /// Summarize and test ModelBasedDistributionSection view, create chart
        /// </summary>
        [TestMethod]
        public void OIMChartCreatorChart_TestCreate() {

            var numIntakes = new[] { 0, 10, 100, 1000 };
            for (int i = 0; i < numIntakes.Length; i++) {
                var number = numIntakes[i];
                var data = NormalDistribution.NormalSamples(number, .5, 1.5).Select(c => Math.Exp(c)).ToList();
                var referenceDose = data.Percentile(82);
                var bins = simulateBins(data, number);
                var section = new ModelBasedDistributionSection() {
                    IntakeDistributionBins = bins,
                    TotalNumberOfIntakes = number,
                    PercentageZeroIntake = 0
                };
                var chart = new OIMChartCreator(section, "mg/kg bw/day");
                RenderChart(chart, $"TestCreate1{number}");
                AssertIsValidView(section);
            }
        }


        /// <summary>
        /// Summarize and test OIMDistributionSection view, create chart
        /// </summary>
        [TestMethod]
        public void OIMChartCreator_TestCreate() {
            var numIntakes = new[] { 0, 10, 100, 1000 };
            for (int i = 0; i < numIntakes.Length; i++) {
                var number = numIntakes[i];
                var data = NormalDistribution.NormalSamples(number, .5, 1.5).Select(c => Math.Exp(c)).ToList();
                var percentiles = new UncertainDataPointCollection<double>();
                percentiles.XValues = GriddingFunctions.GetPlotPercentages();
                percentiles.ReferenceValues = data.PercentilesWithSamplingWeights(null, percentiles.XValues);
                var bins = simulateBins(data, number);
                var section = new OIMDistributionSection() {
                    IntakeDistributionBins = bins,
                    TotalNumberOfIntakes = number,
                    PercentageZeroIntake = 0,
                };
                var chart = new OIMCumulativeChartCreator(section, "mg/kg bw/day");
                    RenderChart(chart, $"TestCreate{ number}.png");
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