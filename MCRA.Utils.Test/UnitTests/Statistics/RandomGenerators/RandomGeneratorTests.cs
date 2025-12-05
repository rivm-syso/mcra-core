using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class RandomGeneratorTests {

        /// <summary>
        /// Test draws from a log-normal distribution using a random generator.
        /// </summary>
        [TestMethod]
        public void TestNumericsDrawFromDrawLogNormal() {
            var nDraws = 2000000;
            var mu = 0.05;
            var sigma = 0.1;
            var seed = 1;

            var random = new McraRandomGenerator(seed);
            var logNormalDistribution = new LogNormalDistribution(mu, sigma);
            var ran = new List<double>(nDraws);
            for (int i = 0; i < nDraws; i++) {
                ran.Add(logNormalDistribution.Draw(random));
            }
            var exactMean = Math.Exp(mu + .5 * Math.Pow(sigma, 2));
            var exactVariance = Math.Exp(2 * mu + Math.Pow(sigma, 2)) * (Math.Exp(Math.Pow(sigma, 2)) - 1);
            Assert.AreEqual(exactMean, ran.Average(), 1e-2);
            Assert.AreEqual(exactVariance, ran.Variance(), 1e-2);
        }

        /// <summary>
        /// Test draws from a Chi-Squared distribution using Numerics random generator.
        /// </summary>
        [TestMethod]
        public void TestNumericsDrawFromChiSquared() {
            var nDraws = 2000000;
            var df = 5;
            var tolMean = 1e-2;
            var tolVar = 1e-2;
            var seed = 1;

            // No changes needed below
            var exactMean = df;
            var exactVar = 2d * df;
            var random = new McraRandomGenerator(seed);
            var chiSquaredDistribution = new ChiSquaredDistribution(df);
            var ran = new List<double>(nDraws);
            for (int i = 0; i < nDraws; i++) {
                ran.Add(chiSquaredDistribution.Draw(random));
            }
            var ranMean = ran.Average();
            var ranVar = ran.Variance();
            var diffMean = Math.Abs((exactMean - ranMean) / exactMean);
            var diffVar = Math.Abs((exactVar - ranVar) / exactVar);
            Assert.IsLessThan(tolMean, diffMean);
            Assert.IsLessThan(tolVar, diffVar);
        }

        /// <summary>
        /// Test draws from a beta distribution using Numerics random generator.
        /// </summary>
        [TestMethod]
        public void TestNumericsDrawFromBetaDistribution() {
            var nDraws = 2000;
            var alfa = .1D;
            var beta = .5D;
            var seed = 1;
            var random = new  McraRandomGenerator(seed);
            var betaDistribution = new BetaDistribution(alfa, beta);
            var ran = new List<double>(nDraws);
            for (int i = 0; i < nDraws; i++) {
                ran.Add(betaDistribution.Draw(random));
            }
            var exactMean = alfa / (alfa + beta);
            var exactVariance = (alfa * beta) / ((alfa + beta + 1) * Math.Pow(alfa + beta, 2));
            Assert.AreEqual(exactMean, ran.Average(), 1e-2);
            Assert.AreEqual(exactVariance, ran.Variance(), 1e-2);
        }
    }
}
