using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test.UnitTests.Charting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class ViolinChartCreatorTests : ChartCreatorTestsBase {

        [TestMethod]
        public void HistogramChartCreator_TestCreateLinear() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var data = NormalDistribution.Samples(random, 0, 1, 100);
            var chartCreator = new HistogramChartCreator(
                data,
                title: "Histogram",
                titleX: "Linear horizontal axis",
                titleY: "Frequency"
            );
            WritePng(chartCreator, $"LinearHorizontalAxis.png");
        }


        /// <summary>
        /// Violin
        /// </summary>
        [TestMethod]
        public void ViolinChart_Test() {
            var data = Mock(100, 200, 300, 400, 500);
            var chart = new ViolinCreator(data, "violinplot", true, true, false);
            WritePng(chart, $"violinplotHorizontalBoxPlot");
            chart = new ViolinCreator(data, "violinplot", true, false, false);
            WritePng(chart, $"violinplotHorizontalPercentiles");
            chart = new ViolinCreator(data, "violinplot", false, true, false);
            WritePng(chart, $"violinplotVerticalBoxPlot");
            chart = new ViolinCreator(data, "violinplot", false, false, false);
            WritePng(chart, $"violinplotVerticalPercentiles1");
        }

        /// <summary>
        /// Mock
        /// </summary>
        /// <param name="numberOfSamples"></param>
        /// <returns></returns>
        public IDictionary<string, List<double>> Mock(params int[] numberOfSamples) {
            var mu = -.5;
            var sigma = .4;
            var result = new Dictionary<string, List<double>>();
            var counter = 0;
            foreach (var count in numberOfSamples) {
                var draw = LogNormalDistribution.LogNormalSamples(count, mu + counter * .1, sigma).ToList();
                result[$"{counter}"] = draw;
                counter++;
            }
            return result;
        }
    }
}