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
        [DataRow(25,5,70,30,20,50,"1")]
        [DataRow(6,44,50,30,20,50,"2")]
        [DataRow(6,44,50,3,20,77,"3")]
        [DataRow(6,5,89,30,20,50,"4_Oth")]
        [DataRow(6,6,88,3,20,77,"5_Oth")]
        [DataRow(6,6,88,3,6,91,"6_Oth")]
        public void StackedBarChart1_Test(
            double maleOral,
            double maleInhalation,
            double maleDermal,
            double femaleOral,
            double femaleInhalation,
            double femaleDermal,
            string chartName
        ) {
            var data = new List<BarDataPoint>() {
                new ("Male", "Oral", maleOral),
                new ("Male", "Inhalation", maleInhalation),
                new ("Male", "Dermal", maleDermal),
                new ("Female", "Oral", femaleOral),
                new ("Female", "Inhalation", femaleInhalation),
                new ("Female", "Dermal", femaleDermal)
            };

            var chart = new StackedBarChartCreator(data);
            WritePng(chart, $"StackedBarChart{chartName}");
        }
    }
}
