using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {
    [TestClass]
    public class ContinuousUniformDistributionTests : DistributionsTestsBase {

        private int _seed = 1;
        private int _ndraws = 10000;
        private double _lower = 3;
        private double _upper = 8D;

        /// <summary>
        /// Using Numerics for ContinuousUniform
        /// </summary>
        [TestMethod]
        public void ContinuousUniformDistribution_TestDrawN() {
            var list = draw(_lower, _upper, _seed, _ndraws);
            var mean = list.Average();
            var mu = (_lower + _upper) / 2;
            Assert.AreEqual(mean, mu, 0.3);
        }

        /// <summary>
        /// Using Numerics for Normal
        /// </summary>
        [TestMethod]
        public void ContinuousUniformDistribution_TestPlot() {
            var list = draw(_lower, _upper, _seed, _ndraws);
            var title = "ContinuousUniform";
            var chartCreator = new HistogramChartCreator(list, title);
            WritePng(chartCreator, title);
        }

        private static List<double> draw(double lower, double upper, int seed, int ndraws) {
            var random = new McraRandomGenerator(seed);
            var distribution = new ContinuousUniformDistribution(lower, upper);
            return distribution.Draws(random, ndraws);
        }
    }
}
