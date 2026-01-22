using MCRA.Utils.Statistics;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {

    /// <summary>
    /// Tests the multivariate normal distribution function.
    /// </summary>
    [TestClass]
    public class MultiVariateNormalDistributionTests : DistributionsTestsBase {

        [TestMethod]
        [DataRow(0.4, 0.5, 0.8, 0.1, 0.2)]
        [DataRow(4, 5, 0.8, 1, 2)]
        [DataRow(0.4, 0.2, 0.6, 0.1, 0.2)]
        [DataRow(4, 5, 0.2, 1, 2)]
        public void MultiVariateNormalDistribution_Test(
            double mu1,
            double mu2,
            double covar,
            double var1,
            double var2
        ) {
            var seed = 1;
            var numSamples = 1000;
            var mean = new List<double> { mu1, mu2 };
            var vcov = new double[,] { { var1, covar * Math.Sqrt(var1 * var2) }, { covar * Math.Sqrt(var1 * var2), var2 } };
            var random = new McraRandomGenerator(seed);

            var mn1 = new List<double>();
            var mn2 = new List<double>();
            var dist = new MultiVariateNormalDistribution(mean, vcov);

            var result = dist.Samples(random, numSamples);
            for (int i = 0; i < numSamples; i++) {
                mn1.Add(result[i][0]);
                mn2.Add(result[i][1]);
            }

            var avg1 = mn1.Average();
            var avg2 = mn2.Average();
            var estVar1 = mn1.Variance();
            var estVar2 = mn2.Variance();
            var correlation = MathNet.Numerics.Statistics.Correlation.Pearson(mn1, mn2);
            Assert.AreEqual(mu1, avg1, 1e-1);
            Assert.AreEqual(mu2, avg2, 1e-1);
            Assert.AreEqual(var1, estVar1, 1e-1);
            Assert.AreEqual(var2, estVar2, 1e-1);
            Assert.AreEqual(covar, correlation, 1e-1);
        }
    }
}
