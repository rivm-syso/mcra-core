using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Utils.Statistics.Histograms;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration.ActionSummaries.TargetExposures {

    /// <summary>
    /// OutputGeneration, ActionSummaries, TargetExposures, ExposureByRoute, ExposureDistribution, DietaryExposureDistribution
    /// </summary>
    [TestClass]
    public class DietaryTotalIntakeCumulativeDistributionChartTest : ChartCreatorTestBase {

        /// <summary>
        /// Test create dietary total intake distrbution chart for a nominal run.
        /// </summary>
        [TestMethod]
        public void DietaryTotalIntakeCumulativeDistributionChart_TestNominal() {
            var number = 5000;
            var logData = NormalDistribution.NormalSamples(number, .5, 1.5).ToList();
            var referenceDose = Math.Pow(10, logData.Percentile(82));
            var bins = simulateBins(logData);

            var section = new DietaryTotalIntakeDistributionSection() {
                IntakeDistributionBins = bins,
                TotalNumberOfIntakes = number,
                PercentageZeroIntake = 20
            };

            var chart = new DietaryTotalIntakeCumulativeDistributionChartCreator(section, "mg/kg bw/day");
            RenderChart(chart, $"TestCreate1");
            AssertIsValidView(section);
        }

        /// <summary>
        /// Test create dietary total intake distrbution chart for a run with uncertainty.
        /// </summary>
        [TestMethod]
        public void DietaryTotalIntakeCumulativeDistributionChart_TestUncertain() {
            var number = 5000;
            var numZeros = 1000;
            var seed = 1;
            var rnd = new McraRandomGenerator(seed);
            var logNormal = new LogNormalDistribution(0.5, 1.5);
            var positives = logNormal.Draws(rnd, number);
            var logPositives = positives.Select(r => Math.Log(r)).ToList();
            var zeros = Enumerable.Repeat(0D, numZeros).ToList();
            var allExposures = positives.Concat(zeros);
            var referenceDose = positives.Percentile(82);
            var percentages = GriddingFunctions.GetPlotPercentages();
            var section = new DietaryTotalIntakeDistributionSection() {
                IntakeDistributionBins = simulateBins(logPositives),
                Percentiles = new UncertainDataPointCollection<double>(percentages),
                TotalNumberOfIntakes = number,
                PercentageZeroIntake = ((double)numZeros/number) * 100,
                UncertaintyLowerlimit = 2.5,
                UncertaintyUpperlimit = 97.5
            };
            section.Percentiles.ReferenceValues = allExposures.Percentiles(percentages);

            for (int i = 0; i < 10; i++) {
                var mu = 0 + 1 * rnd.NextDouble();
                positives = LogNormalDistribution.Samples(rnd, mu, 1.5, number);
                allExposures = positives.Concat(zeros);
                section.Percentiles.AddUncertaintyValues(allExposures.Percentiles(section.Percentiles.XValues));
            }

            var chart = new DietaryTotalIntakeCumulativeDistributionChartCreator(section, "mg/kg bw/day");
            RenderChart(chart, $"TestCreate2");
            AssertIsValidView(section);
        }

        private List<HistogramBin> simulateBins(List<double> data) {
            var weights = data.Select(r => 1D).ToList();
            int numberOfBins = Math.Sqrt(data.Count) < 100 ? BMath.Ceiling(Math.Sqrt(data.Count)) : 100;
            return data.MakeHistogramBins(weights, numberOfBins, data.Min(), data.Max());
        }
    }
}