using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test.UnitTests.Charting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class HistogramChartCreatorTests : ChartCreatorTestsBase {

        [TestMethod]
        public void HistogramChartCreator_TestCreateLinear() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var data = NormalDistribution.Samples(random, 0, 1, 100000);
            var chartCreator = new HistogramChartCreator(
                data,
                title: "Histogram",
                titleX: "Linear horizontal axis",
                titleY: "Frequency"
            );
            WritePng(chartCreator, $"LinearHorizontalAxis.png");
        }

        [TestMethod]
        public void HistogramChartCreator_TestCreateLogarithmic() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var data = NormalDistribution
                .Samples(random, 0.5, 1.5, 100000)
                .Select(c => Math.Pow(c, 10))
                .ToList();
            var chartCreator = new HistogramChartCreator(
                data, 
                title: "Histogram", 
                titleX: "Logarithmic base 10 axis",
                titleY: "Frequency",
                isLogarithmic: true
            );
            WritePng(chartCreator, $"LogBase10HorizontalAxis.png");
        }
    }
}