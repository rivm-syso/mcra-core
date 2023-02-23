using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {
    [TestClass]
    public class LogNormalDistributionTests : DistributionsTestsBase {

        private int _seed = 1;
        private int _ndraws = 100000;
        private double _mu = 1.853;
        private double _sigma = 0.325;
        private double _offset = 0;

        /// <summary>
        /// Using Numerics for LogNormal
        /// </summary>
        [TestMethod]
        public void LogNormalDistribution_TestDrawN() {
            var logNormalList = draw(_mu, _sigma, _seed, _ndraws, _offset);
            var mu = _mu;

            //op logscale
            var mean = logNormalList.Average(c=> Math.Log(c));
            Assert.AreEqual(mu, mean, 0.3);

            //op lognormal scale
            var testmean = logNormalList.Average();
            var testMu = Math.Exp(_mu + (_sigma * _sigma / 2.0));
            Assert.AreEqual(testMu, testmean, 0.3);
        }

        /// <summary>
        /// Using Numerics for LogNormal
        /// </summary>
        [TestMethod]
        public void LogNormalDistribution_TestPlot() {
            var list = draw(_mu, _sigma, _seed, _ndraws, _offset);
            var title = "LogNormal";
            var logList = list.Select(c => c = Math.Log(c)).ToList();
            var chartCreator = new HistogramChartCreator(logList, title);
            WritePng(chartCreator, title);
        }

        [TestMethod]
        public void LogNormalDistribution_TestParametersFromMeanVariance() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var mean = 2;
            var variance = .3;

            // Based on the mean and variance, the parameters of the log-normal distribution
            // can be calculated as follows:
            var mu = Math.Log(Math.Pow(mean, 2) / Math.Sqrt(variance + Math.Pow(mean, 2)));
            var sigma = Math.Sqrt(Math.Log(1 + variance / (Math.Pow(mean, 2))));

            // Draw n values from the lognormal distribution with mu and sigma
            var values = new List<double>();
            for (int i = 0; i < 100000; i++) {
                values.Add(LogNormalDistribution.Draw(random, mu, sigma));
            }

            // Check sample statistics
            var sampleMean = values.Average();
            var sampleVariance = values.Variance();
            Assert.AreEqual(sampleMean, mean, 1e-2);
            Assert.AreEqual(sampleVariance, variance, 1e-2);

            var gm = Math.Exp(values.Average(r => Math.Log(r)));
            var gvar = Math.Exp(values.Select(r => Math.Log(r)).ToList().Variance());

            Assert.AreEqual(Math.Exp(mu), gm, 1e-2);
            Assert.AreEqual(Math.Exp(Math.Pow(sigma, 2)), gvar, 1e-2);
        }

        private static List<double> draw(double mu, double sigma, int seed, int ndraws, double offset = 0) {
            var random = new McraRandomGenerator(seed);
            var distribution = new LogNormalDistribution(mu, sigma, offset);
            return distribution.Draws(random, ndraws);
        }
    }
}
