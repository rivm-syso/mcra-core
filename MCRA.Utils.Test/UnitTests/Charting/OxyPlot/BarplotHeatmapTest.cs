using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Test.UnitTests.Charting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class BarplotHeatmapTest : ChartCreatorTestsBase {

        /// <summary>
        /// Make bar chart and select all substances with marginal contributions
        /// </summary>
        [TestMethod]
        public void BarplotChartCreator_Test() {
            var data1 = new List<(string name, double contribution)> { ("B", .1), ("A", .2), ("C", .4), ("D", .8), ("E", .8), ("F", .8) };
            var chartCreator = new BarChartCreator(
                "Component 1",
                data1
            );
            WritePng(chartCreator, $"BarChart1");
        }
    }
}

