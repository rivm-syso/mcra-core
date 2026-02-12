using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test.UnitTests.Charting;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class ViolinChartCreatorTests : ChartCreatorTestsBase {

        /// <summary>
        /// Violin
        /// </summary>
        [TestMethod]
        public void ViolinChart_Test() {
            var data = fakeData(100, 200, 300, 400, 500);
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
        /// Creates fake data.
        /// </summary>
        /// <param name="numberOfSamples"></param>
        /// <returns></returns>
        private IDictionary<string, List<double>> fakeData(params int[] numberOfSamples) {
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
