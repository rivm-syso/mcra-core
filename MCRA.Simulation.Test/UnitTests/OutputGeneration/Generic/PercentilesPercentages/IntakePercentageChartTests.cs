using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MCRA.Utils.Test;

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
        public void IntakePercentageChart_TestUncertainty() {
            var section = new IntakePercentageSection() {
                Percentages = Mock(100),
            };
            var chart = new IntakePercentageChartCreator(section, "mg/kg");
            chart.CreateToSvg(TestUtilities.ConcatWithOutputPath("IntakePercentage_TestUncertainty.svg"));
        }
        /// <summary>
        /// Mock random
        /// </summary>
        /// <param name="numberOfSamples"></param>
        /// <returns></returns>
        public UncertainDataPointCollection<double> Mock(int numberOfSamples) {
            var percentages = new List<double>(){50, 90, 95, 99, 99.5, 99.9};
            var mu = 110.5;
            var sigma = 5;
            var draw = NormalDistribution.NormalSamples(numberOfSamples, mu, sigma).ToList();
            var collection = new UncertainDataPointCollection<double>() {
                XValues = percentages,
                ReferenceValues = draw.Percentiles(percentages),
            };
            for (int i = 0; i < 10; ++i) {
                draw = NormalDistribution.NormalSamples(numberOfSamples, mu, sigma).ToList();
                collection.AddUncertaintyValues(draw.Percentiles(percentages));
            }
            return collection;
        }
    }
}
