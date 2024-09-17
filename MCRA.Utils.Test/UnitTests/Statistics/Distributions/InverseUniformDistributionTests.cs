using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {
    [TestClass]
    public class InverseUniformDistributionTests : DistributionsTestsBase {

        [TestMethod]
        [DataRow(0.05, 0.95)]
        public void InverseUniformDistribution_TestDraw(double a, double b) {
            var distribution = new InverseUniformDistribution(a, b);
            var n = 1000;
            var random = new McraRandomGenerator(1);
            for (int i = 0; i < n; i++) {
                var val = distribution.Draw(random);
                Assert.IsTrue(val <= 1/a);
                Assert.IsTrue(val >= 1/b);
            }
        }

        [TestMethod]
        [DataRow(0.5, 0.95)]
        [DataRow(3, 20)]
        [DataRow(8, 16)]
        public void InverseUniformDistribution_TestFromMeanUpper(double mean, double upper) {
            var distribution = InverseUniformDistribution.FromMeanAndUpper(mean, upper);
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
        [DataRow(1.8, 18)]
        public void InverseUniformDistribution_TestFromMedianUpper(double median, double upper) {
            var distribution = InverseUniformDistribution.FromMedianAndUpper(median, upper);
            var n = 20000;
            var random = new McraRandomGenerator(1);
            var draws = new List<double>();
            for (int i = 0; i < n; i++) {
                draws.Add(distribution.Draw(random));
            }
            Assert.IsTrue(draws.All(val => val <= upper));
            Assert.AreEqual(median, draws.Median(), 1e-1);
        }
    }
}
