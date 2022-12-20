using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {

    [TestClass]
    public class BernoulliDistributionTests : DistributionsTestsBase {
        private int _seed = 1;
        private int _ndraws = 10000;
        private double _mu = .3;

        /// <summary>
        /// Using Numerics for Bernoulli
        /// </summary>
        [TestMethod]
        public void BernoulliDistribution_TestDrawN() {
            var list = draw(_mu, _seed, _ndraws);
            var mean = list.Average();
            Assert.AreEqual(mean, _mu, 0.3);
        }

        /// <summary>
        /// Using Numerics for Bernoulli
        /// </summary>
        [TestMethod]
        public void BernoulliDistribution_TestPlot() {
            var list = draw(_mu, _seed, _ndraws);
            var title = "Bernoulli";
            var chartCreator = new HistogramChartCreator(list, title);
            WritePng(chartCreator, title);
        }

        private static List<double> draw(double mu, int seed, int ndraws) {
            var random = new McraRandomGenerator(seed);
            var distribution = new BernoulliDistribution(mu);
            return distribution.Draws(random, ndraws);
        }
    }
}
