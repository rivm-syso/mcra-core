using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {
    [TestClass]
    public class NormalDistributionTests : DistributionsTestsBase {

        private int _seed = 1;
        private int _ndraws = 1000;
        private double _mu = 3;
        private double _sigma = 2D;

        /// <summary>
        /// Using Numerics for Normal
        /// </summary>
        [TestMethod]
        public void NormalDistribution_TestDrawN() {
            //do loop when first time fails
            //turf how many in loop fails
            //1%
            var list = draw(_mu, _sigma, _seed, _ndraws);
            var mean = list.Average();
            var mu = _mu;
            var tol = 2.32 * _sigma / Math.Sqrt(_ndraws);
            Assert.AreEqual(mu, mean, tol);
        }

        /// <summary>
        /// Test draws from a log-normal distribution using RMath.
        /// </summary>
        [TestMethod]
        public void NormalDistribution_TestDraw() {
            var nDraws = 2000000;
            var mu = 0.05;
            var sigma = 0.1;
            var ran = new List<double>(nDraws);
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            for (int i = 0; i < nDraws; i++) {
                ran.Add(Math.Exp(NormalDistribution.Draw(random, mu, sigma)));
            }
            var exactMean = Math.Exp(mu + .5 * Math.Pow(sigma, 2));
            var exactVariance = Math.Exp(2 * mu + Math.Pow(sigma, 2)) * (Math.Exp(Math.Pow(sigma, 2)) - 1);
            Assert.AreEqual(exactMean, ran.Average(), 1e-2);
            Assert.AreEqual(exactVariance, ran.Variance(), 1e-2);
        }

        /// <summary>
        /// Test draws from a log-normal distribution using RMath.
        /// </summary>
        [TestMethod]
        public void NormalDistribution_TestDrawInvCdf() {
            var nDraws = 2000000;
            var mu = 0.05;
            var sigma = 0.1;
            var ran = new List<double>(nDraws);
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            for (int i = 0; i < nDraws; i++) {
                ran.Add(Math.Exp(NormalDistribution.DrawInvCdf(random, mu, sigma)));
            }
            var exactMean = Math.Exp(mu + .5 * Math.Pow(sigma, 2));
            var exactVariance = Math.Exp(2 * mu + Math.Pow(sigma, 2)) * (Math.Exp(Math.Pow(sigma, 2)) - 1);
            Assert.AreEqual(exactMean, ran.Average(), 1e-2);
            Assert.AreEqual(exactVariance, ran.Variance(), 1e-2);
        }

        /// <summary>
        /// Using Numerics for Normal
        /// </summary>
        [TestMethod]
        public void NormalDistribution_TestPlot() {
            var list = draw(_mu, _sigma, _seed, _ndraws);
            var title = "Normal";
            var chartCreator = new HistogramChartCreator(list, title);
            WritePng(chartCreator, title);
        }

        /// <summary>
        /// Test PDF function.
        /// </summary>
        [TestMethod]
        [DataRow(0D, 1D, 0D)]
        [DataRow(0D, 1D, 6D)]
        [DataRow(-6D, 1D, 6D)]
        [DataRow(100D, 5D, 0D)]
        public void NormalDistribution_TestPdf(double mu, double sigma, double x) {
            var expected = 1.0 / Math.Sqrt(2 * Math.PI) / sigma * Math.Exp(-Math.Pow((x - mu), 2) / (2 * sigma * sigma));
            var computed = NormalDistribution.PDF(mu, sigma, x);
            Assert.AreEqual(expected, computed, 1e-6);
        }

        private static List<double> draw(double mu, double sigma, int seed, int ndraws) {
            var random = new McraRandomGenerator(seed);
            var distribution = new NormalDistribution(mu, sigma);
            return distribution.Draws(random, ndraws);
        }
    }
}
