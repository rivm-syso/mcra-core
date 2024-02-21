using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {

    /// <summary>
    /// Tests the Multinomial distribution function
    /// </summary>
    [TestClass()]
    public class MultinomialDistributionTests : DistributionsTestsBase {

        private static double _epsilon = 1e-12;

        /// <summary>
        /// Tests the Multinomial distribution function with an input array of zeroes.
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void MultinomialDistribution_TestScales() {
            var prob = new double[] { 0.1, 0.2, 0.3, 0.4 };
            var numbers = new[] { 1, 10, 100, 1000 };
            foreach (var number in numbers) {
                var shares = MultinomialDistribution.Sample(prob, 1, number);
                Assert.AreEqual(number, shares.Sum(), _epsilon);
            }
        }
    }
}
