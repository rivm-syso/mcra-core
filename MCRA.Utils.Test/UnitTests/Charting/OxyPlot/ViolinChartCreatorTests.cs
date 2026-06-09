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
        [DataRow(true, true, false, "HorizontalBoxPlot")]
        [DataRow(true, false, false, "HorizontalPercentiles")]
        [DataRow(false, true, false, "VerticalBoxPlot")]
        [DataRow(false, false, false, "VerticalPercentiles")]
        public void ViolinChart_Test(bool horizontal, bool item, bool equal, string chartName) {
            var data = fakeData(100, 200, 300, 400, 500);
            var chart = new ViolinCreator(data, "violinplot", horizontal, item, equal);
            WritePng(chart, $"violinplot{chartName}");
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
