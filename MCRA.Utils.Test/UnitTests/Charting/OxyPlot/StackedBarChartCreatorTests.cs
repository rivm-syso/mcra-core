using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test.UnitTests.Charting;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class StackedBarChartCreatorTests : ChartCreatorTestsBase {

        /// <summary>
        /// Violin
        /// </summary>
        [TestMethod]
        public void StackedBarChart_Test() {
            var data = new List<BarDataPoint>() {
                new ("Male", "Oral", 25),
                new ("Male", "Inhalation", 5),
                new ("Male", "Dermal", 70),
                new ("Female", "Oral", 30),
                new ("Female", "Inhalation", 20),
                new ("Female", "Dermal", 50)
            };

            var chart = new StackedBarChartCreator(data);
            WritePng(chart, $"StackedBarChart");
        }
    }
}
