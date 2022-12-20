using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {
    [TestClass]
    public class GammaDistributionTests : DistributionsTestsBase {

        private int _seed = 1;
        private int _ndraws = 10000;
        private double _shape = 3;
        private double _rate = 2D;
        private double _scale = 2D;

        /// <summary>
        /// Using Numerics for bernouilli
        /// </summary>
        [TestMethod]
        public void GammaDistribution_TestDrawN() {
            var list = draw(_shape, _rate, _scale, _seed, _ndraws);
            var mu = _shape / _rate + _scale;
            var mean = list.Average();
            Assert.AreEqual(mu, mean, 0.3);
        }

        /// <summary>
        /// Using Numerics for bernouilli
        /// </summary>
        [TestMethod]
        public void GammaDistribution_TestPlot() {
            var list = draw(_shape, _rate, _scale, _seed, _ndraws);
            var title = "Gamma";
            var chartCreator = new HistogramChartCreator(list, title);
            WritePng(chartCreator, title);
        }

        private static List<double> draw(double shape, double rate, double scale, int seed, int ndraws) {
            var random = new McraRandomGenerator(seed);
            var distribution = new GammaDistribution(shape, rate, scale);
            return distribution.Draws(random, ndraws);
        }
    }
}
