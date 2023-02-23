using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {
    [TestClass]
    public class DiscreteUniformDistributionTests : DistributionsTestsBase {

        private int _seed = 1;
        private int _ndraws = 10000;
        private int _lower = 3;
        private int _upper = 9;

        /// <summary>
        /// Using Numerics for DiscreteUniform
        /// </summary>
        [TestMethod]
        public void DiscreteUniformDistribution_TestDrawN() {
            var list = draw(_lower, _upper, _seed, _ndraws);
            var mean = list.Average();
            var mu = (_lower + _upper) / 2;
            Assert.AreEqual(mean, mu, 0.3);
        }

        /// <summary>
        /// Using Numerics for DiscreteContinuous
        /// </summary>
        [TestMethod]
        public void DiscreteUniformDistribution_TestPlot() {
            var list = draw(_lower, _upper, _seed, _ndraws);
            var title = "DiscreteUniform";
            var chartCreator = new HistogramChartCreator(list, title);
            WritePng(chartCreator, title);
        }

        private static List<double> draw(int lower, int upper, int seed, int ndraws) {
            var random = new McraRandomGenerator(seed);
            var distribution = new DiscreteUniformDistribution(lower, upper);
            return distribution.Draws(random, ndraws);
        }
    }
}
