using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {
    [TestClass]
    public class BetaDistributionTests : DistributionsTestsBase {

        private int _seed = 1;
        private int _ndraws = 10000;
        private double _shapeA = 3;
        private double _shapeB = 3;

        /// <summary>
        /// Using Numerics for bernouilli
        /// </summary>
        [TestMethod]
        public void BetaDistribution_TestDrawN() {
            var list = draw(_shapeA, _shapeB, _seed, _ndraws);
            var mu = _shapeA / (_shapeA + _shapeB);
            var mean = list.Average();
            Assert.AreEqual(mu, mean, 0.3);
        }

        /// <summary>
        ///  Test draws from a beta distribution using RMath.
        /// </summary>
        [TestMethod]
        public void BetaDistribution_TestDraw() {
            var nDraws = 20000;
            var alfa = .1D;
            var beta = .5D;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            List<double> ran = new List<double>(nDraws);
            for (int i = 0; i < nDraws; i++) {
                ran.Add(BetaDistribution.Draw(random, alfa, beta));
            }
            var exactMean = alfa / (alfa + beta);
            var exactVariance = (alfa * beta) / ((alfa + beta + 1) * Math.Pow(alfa + beta, 2));
            var observedMean = ran.Average();
            var observedVariance = ran.Variance();
            Assert.AreEqual(exactMean, observedMean, 1e-2);
            Assert.AreEqual(exactVariance, observedVariance, 1e-2);
        }

        /// <summary>
        /// Using Numerics for Beta
        /// </summary>
        [TestMethod]
        public void BetaDistribution_TestPlot() {
            var list = draw(_shapeA, _shapeB, _seed, _ndraws);
            var title = "Beta";
            var chartCreator = new HistogramChartCreator(list, title);
            WritePng(chartCreator, title);
        }

        private static List<double> draw(double shapeA, double shapeB, int seed, int ndraws) {
            var random = new McraRandomGenerator(seed);
            var distribution = new BetaDistribution(shapeA, shapeB);
            return distribution.Draws(random, ndraws);
        }
    }
}
