using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {
    [TestClass]
    public class TriangularDistributionTests : DistributionsTestsBase {

        /// <summary>
        /// Using Numerics for Triangular
        /// </summary>
        [TestMethod]
        [DataRow(3, 1, 6)]
        [DataRow(4, 1, 6)]
        [DataRow(5, 1, 7)]
        [DataRow(6, 1, 7)]
        public void TriangularDistribution_TestDrawN(
            double mode,
            double lower,
            double upper
        ) {

            var distribution = new TriangularDistribution(lower, upper, mode);
            var random = new McraRandomGenerator(1);
            var values = distribution.Draws(random, 10000);

            var sampleMean = values.Average();
            Assert.AreEqual(distribution.Mean, sampleMean, 0.3);
            var sampleVariance = values.Variance();
            Assert.AreEqual(distribution.Variance, sampleVariance, 0.3);

        }

        /// <summary>
        /// Using Numerics for Triangular
        /// </summary>
        [TestMethod]
        public void TriangularDistribution_TestPlot() {
            var mode = 3;
            var lower = 1;
            var upper = 6;
            var distribution = new TriangularDistribution(lower, upper, mode);

            var random = new McraRandomGenerator(1);
            var values = distribution.Draws(random, 10000);

            var title = "Triangular";
            var chartCreator = new HistogramChartCreator(values, title);
            WritePng(chartCreator, title);
        }
    }
}
