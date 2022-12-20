using MCRA.Utils.Statistics;
using MCRA.Simulation.OutputGeneration;
using MCRA.Simulation.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.Test.UnitTests.OutputGeneration {
    /// <summary>
    /// IntakePercentileChartTests
    /// </summary>
    [TestClass]
    public class IntakePercentileChartTests {
        /// <summary>
        /// IntakepercentileChart_TestUncertainty
        /// </summary>
        [TestMethod]
        public void IntakepercentileChart_TestUncertainty() {
            var section = new IntakePercentileSection() {
                Percentiles = Mock(100),
            };
            var chart = new IntakePercentileChartCreator(section, "mg/kg");
            chart.CreateToSvg(TestResourceUtilities.ConcatWithOutputPath("IntakePercentile_TestUncertainty.svg"));
        }

        /// <summary>
        /// Mock
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
