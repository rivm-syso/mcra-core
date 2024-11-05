using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {
    [TestClass]
    public class LogNormalDistributionTests : DistributionsTestsBase {

        /// <summary>
        /// Using Numerics for LogNormal
        /// </summary>
        [TestMethod]
        public void LogNormalDistribution_TestDrawN() {
            var mu = 1.853;
            var sigma = 0.325;
            var distribution = new LogNormalDistribution(mu, sigma, 0);

            var random = new McraRandomGenerator(1);
            var values = distribution.Draws(random, 100000);

            var mean = values.Average(c=> Math.Log(c));
            Assert.AreEqual(mu, mean, 0.3);

            var testmean = values.Average();
            var testMu = Math.Exp(mu + (sigma * sigma / 2.0));
            Assert.AreEqual(testMu, testmean, 0.3);
        }

        /// <summary>
        /// Using Numerics for LogNormal
        /// </summary>
        [TestMethod]
        public void LogNormalDistribution_TestPlot() {
            var mu = 1.853;
            var sigma = 0.325;
            var distribution = new LogNormalDistribution(mu, sigma, 0);

            var random = new McraRandomGenerator(1);
            var values = distribution.Draws(random, 100000);

            var title = "LogNormal";
            var logList = values.Select(c => c = Math.Log(c)).ToList();
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

        [TestMethod]
        [DataRow(2, .3)]
        public void LogNormalDistribution_TestFromMeanAndCv(
            double expectedMu,
            double expectedSigma
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var samples = LogNormalDistribution.Samples(random, expectedMu, expectedSigma, 1000);
            var mean = samples.Average();
            var cv = samples.CV();

            var distribution = LogNormalDistribution.FromMeanAndCv(mean, cv);
            Assert.AreEqual(expectedMu, distribution.Mu, 0.01);
            Assert.AreEqual(expectedSigma, distribution.Sigma, 0.01);
        }
    }
}
