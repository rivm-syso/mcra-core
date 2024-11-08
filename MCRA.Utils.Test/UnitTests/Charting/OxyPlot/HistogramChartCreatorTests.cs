using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test.UnitTests.Charting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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

        [TestMethod]
        public void HistogramChartCreator_TestCreateLeftTail() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var mu = Math.Log(0.3);
            var sigma = 1.2;
            var lod = 0.0;
            var loq = .06;
            var pLod = LogNormalDistribution.CDF(mu, sigma, lod);
            var pLoq = LogNormalDistribution.CDF(mu, sigma, loq);
            var minValue = pLod / pLoq;
            var n = 10000;
            var data = new List<double>();
            for (int i = 0; i < n; i++) {
                var draw = LogNormalDistribution.InvCDF(mu, sigma, pLoq * random.NextDouble(minValue, 1d));
                data.Add(draw);
            }
            var chartCreator = new HistogramChartCreator(
                data,
                title: "HistogramTail",
                true,
                titleX: "Logarithmic horizontal axis",
                titleY: "Frequency"
            );

            WritePng(chartCreator, $"LeftTailLogNormal");
        }

        [TestMethod]
        public void HistogramChartCreator_TestCreateSegmentTail() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var mu = Math.Log(0.3);
            var sigma = 1.2;
            var lod = 0.02;
            var loq = 0.06;
            var pLod = LogNormalDistribution.CDF(mu, sigma, lod);
            var pLoq = LogNormalDistribution.CDF(mu, sigma, loq);
            var minValue = pLod / pLoq;
            var n = 10000;
            var data = new List<double>();
            for (int i = 0; i < n; i++) {
                var draw = LogNormalDistribution.InvCDF(mu, sigma, pLoq * random.NextDouble(minValue, 1d));
                data.Add(draw);
            }
            var chartCreator = new HistogramChartCreator(
                data,
                title: "HistogramSegment",
                true,
                titleX: "Logarithmic horizontal axis",
                titleY: "Frequency"
            );
            WritePng(chartCreator, $"LeftTailSegmentLogNormal");
        }
    }
}