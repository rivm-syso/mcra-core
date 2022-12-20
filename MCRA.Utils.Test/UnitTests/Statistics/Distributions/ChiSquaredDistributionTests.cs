using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using MCRA.Utils.Test.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {
    [TestClass]
    public class ChiSquaredDistributionTests : DistributionsTestsBase {

        private int _seed = 1;
        private int _ndraws = 10000;
        private double _freedom = 3;

        /// <summary>
        /// Using Numerics for ChiSquared
        /// </summary>
        [TestMethod]
        public void ChiSquaredDistribution_TestDrawN() {
            var list = draw(_freedom, _seed, _ndraws);
            var mu = _freedom;
            var mean = list.Average();
            Assert.AreEqual(mu, mean, 0.3);
        }

        /// <summary>
        /// Test draws from a Chi-Squared distribution using RMath.
        /// </summary>
        [TestMethod]
        public void TestRMathChiSquared() {
            var nDraws = 2000000;
            var df = 5;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var ran = new List<double>(nDraws);
            for (int i = 0; i < nDraws; i++) {
                ran.Add(ChiSquaredDistribution.Draw(random, df));
            }

            // Assert sample mean is approx. degrees of freedom
            Assert.AreEqual(df, ran.Average(), 1e-2);

            // Assert variance is approx. 2 * degrees of freedom
            Assert.AreEqual(2 * df, ran.Variance(), 1e-2);
        }

        /// <summary>
        /// Using Numerics for ChiSquared
        /// </summary>
        [TestMethod]
        public void ChiSquaredaDistribution_TestPlot() {
            var list = draw(_freedom, _seed, _ndraws);
            var title = "ChiSquared";
            var chartCreator = new HistogramChartCreator(list, title);
            WritePng(chartCreator, title);
        }

        private static List<double> draw(double freedom, int seed, int ndraws) {
            var random = new McraRandomGenerator(seed);
            var distribution = new ChiSquaredDistribution(freedom);
            return distribution.Draws(random, ndraws);
        }
    }
}
