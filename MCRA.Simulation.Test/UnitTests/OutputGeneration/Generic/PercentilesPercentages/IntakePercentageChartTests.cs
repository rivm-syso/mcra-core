using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Simulation.Test.Helpers;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration {
    /// <summary>
    /// OutputGeneration, Generic, PercentilesPercentages
    /// </summary>
    [TestClass]
    public class IntakePercentageChartTests {

        /// <summary>
        /// Create chart
        /// </summary>
        [TestMethod]
        [DataRow(5.5, 1.1)]
        [DataRow(0.5, 0.1)]
        [DataRow(0.05, 0.01)]
        public void IntakePercentageChart_TestUncertainty(double mu, double sigma) {
            var section = new IntakePercentageSection() {
                Percentages = mock(100, mu, sigma),
            };
            var chart = new IntakePercentageChartCreator(section, "mg/kg");
            chart.CreateToSvg(TestUtilities.ConcatWithOutputPath($"IntakePercentage_TestUncertainty_{mu}_{sigma}.svg"));
        }

        private UncertainDataPointCollection<double> mock(int numberOfSamples, double mu, double sigma) {
            var percentages = new List<double>() { 50, 90, 95, 99, 99.5, 99.9 };
            var draw = LogNormalDistribution.LogNormalSamples(numberOfSamples, mu, sigma).ToList();
            var collection = new UncertainDataPointCollection<double>() {
                XValues = percentages,
                ReferenceValues = draw.Percentiles(percentages),
            };
            for (int i = 0; i < 10; ++i) {
                draw = LogNormalDistribution.LogNormalSamples(numberOfSamples, mu, sigma).ToList();
                collection.AddUncertaintyValues(draw.Percentiles(percentages));
            }
            return collection;
        }
    }
}
