using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {
    [TestClass]
    public class UniformDistributionTests : DistributionsTestsBase {

        [TestMethod]
        [DataRow(0.05, 0.95)]
        public void UniformDistribution_TestDraw(double a, double b) {
            var distribution = new UniformDistribution(a, b);
            var n = 1000;
            var random = new McraRandomGenerator(1);
            for (int i = 0; i < n; i++) {
                var val = distribution.Draw(random);
                Assert.IsTrue(val >= a);
                Assert.IsTrue(val <= b);
            }
        }

        [TestMethod]
        [DataRow(0.5, 0.95)]
        [DataRow(3, 20)]
        [DataRow(8, 16)]
        public void UniformDistribution_TestFromMeanUpper(double mean, double upper) {
            var distribution = UniformDistribution.FromMeanAndUpper(mean, upper);
            var n = 100000;
            var random = new McraRandomGenerator(1);
            var draws = new List<double>();
            for (int i = 0; i < n; i++) {
                draws.Add(distribution.Draw(random));
            }
            Assert.IsTrue(draws.All(val => val <= upper));
            var avg = draws.Average();
            Assert.AreEqual(mean, avg, 1e-1);
        }

        [TestMethod]
        [DataRow(0.5, 0.95)]
        [DataRow(8, 16)]
        [DataRow(10, 20)]
        public void UniformDistribution_TestFromMedianUpper(double median, double upper) {
            var distribution = UniformDistribution.FromMedianAndUpper(median, upper);
            var n = 100000;
            var random = new McraRandomGenerator(1);
            var draws = new List<double>();
            for (int i = 0; i < n; i++) {
                draws.Add(distribution.Draw(random));
            }
            Assert.IsTrue(draws.All(val => val <= upper));
            Assert.AreEqual(median, draws.Median(), 1e-1);
        }

        [TestMethod]
        [DataRow(8, 16)]
        public void LogNormalDistribution_TestFromMeanAndCv(
            double expectedLower,
            double expectedUpper
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var samples = UniformDistribution.Samples(random, expectedLower, expectedUpper, 100000);
            var mean = samples.Average();
            var cv = samples.CV();

            var distribution = UniformDistribution.FromMeanAndCv(mean, cv);
            Assert.AreEqual(expectedLower, distribution.Lower, 0.1);
            Assert.AreEqual(expectedUpper, distribution.Upper, 0.1);
        }
    }
}
