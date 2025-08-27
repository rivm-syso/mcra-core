using MCRA.Simulation.Calculators.MarketSharesCalculation;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.Test.UnitTests.Calculators.MarketSharesCalculation {
    [TestClass()]
    public class MarketSharesCalculatorTests {

        /// <summary>
        /// Tests the Dirichlet distribution function with an input array of zeroes with brandloyalty restrictions.
        /// </summary>
        [TestMethod()]
        [DataRow(1, true)]
        [DataRow(1, false)]
        [DataRow(2, true)]
        [DataRow(2, false)]
        [DataRow(4, true)]
        [DataRow(4, false)]
        [DataRow(10, true)]
        [DataRow(10, false)]
        public void MarketSharesCalculator_TestBrandLoyalty(int numProducts, bool isComplete) {
            var brandLoyalties = new[] { 1, .9999, .999, .99, .5, .2, .1, .01, .001, 0.0001, 0.00001, 0 };
            var n = 1;
            for (int i = 0; i < n; i++) {
                var r = new McraRandomGenerator(i);
                var marketShares = FakeMarketShareProbabilities(numProducts, isComplete, r);
                foreach (var brandLoyalty in brandLoyalties) {
                    var result = MarketSharesCalculator.SampleBrandLoyalty(marketShares, brandLoyalty, r.Next());
                    if (isComplete) {
                        Assert.AreEqual(1, result.Sum(), 1e-12);
                    } else {
                        Assert.IsTrue(result.Sum() >= 0 && result.Sum() <= 1);
                    }
                    Assert.AreEqual(marketShares.Length, result.Length);
                }
            }
        }

        /// <summary>
        /// Test sample with absolute brand loyalty. Should produce a market shares sample
        /// draws with only one element with a value of 1 and 0 for the other elements.
        /// </summary>
        [TestMethod()]
        [DataRow(true)]
        [DataRow(false)]
        public void MarketSharesCalculator_TestAbsoluteBrandLoyalty(bool isComplete) {
            var r = new McraRandomGenerator(1);
            var numProds = new[] { 1, 2, 3, 4, 5, 10 };
            var n = 10;
            foreach (var numProducts in numProds) {
                for (int i = 0; i < n; i++) {
                    var marketShares = FakeMarketShareProbabilities(numProducts, isComplete, r);
                    var result = MarketSharesCalculator.SampleBrandLoyalty(marketShares, 1, r.Next());
                    if (isComplete) {
                        Assert.AreEqual(1, result.Sum(), 1e-12);
                        Assert.AreEqual(1, result.Count(r => r > .99));
                    } else {
                        Assert.IsTrue(result.Sum() >= 0 && result.Sum() <= 1);
                    }
                    Assert.AreEqual(marketShares.Length, result.Length);
                }
            }
        }

        /// <summary>
        /// Test sample with no brand loyalty. Should produce a market shares sample
        /// draws rwith only one element with a value of 1 and 0 for the other elements.
        /// </summary>
        [TestMethod()]
        [DataRow(true)]
        [DataRow(false)]
        public void MarketSharesCalculator_TestNoBrandLoyalty(bool isComplete) {
            var r = new McraRandomGenerator(1);
            var numProds = new[] { 1, 2, 3, 4, 5, 10 };
            var n = 10;
            foreach (var numProducts in numProds) {
                for (int i = 0; i < n; i++) {
                    var marketShares = FakeMarketShareProbabilities(numProducts, isComplete, r);
                    var result = MarketSharesCalculator.SampleBrandLoyalty(marketShares, 0, r.Next());
                    if (isComplete) {
                        Assert.AreEqual(1, result.Sum(), 1e-12);
                        for (int j = 0; j < marketShares.Length; j++) {
                            Assert.AreEqual(marketShares[j], result[j], 1e-12);
                        }
                    } else {
                        Assert.IsTrue(result.Sum() >= 0 && result.Sum() <= 1);
                    }
                    Assert.AreEqual(marketShares.Length, result.Length);
                }
            }
        }

        private static double[] FakeMarketShareProbabilities(int n, bool isComplete, IRandom random) {
            var result = Enumerable.Range(1, n).Select(r => random.NextDouble()).ToArray();
            var sum = result.Sum();
            if (!isComplete) {
                sum += random.NextDouble();
            }
            result = result.Select(r => r / sum).ToArray();
            return result;
        }
    }
}
