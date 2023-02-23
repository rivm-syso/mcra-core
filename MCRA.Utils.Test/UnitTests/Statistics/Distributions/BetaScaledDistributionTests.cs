using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {
    [TestClass]
    public class BetaScaledDistributionTests : DistributionsTestsBase {

        private int _seed = 1;
        private int _ndraws = 10000;
        private double _shapeA = 2.1;
        private double _shapeB = 3;
        private double _location = 3.2;
        private double _scale = 1.9;

        /// <summary>
        /// Using Numerics for BetaScaled
        /// </summary>
        [TestMethod]
        public void BetaScaledDistribution_TestDrawN() {
            var list = draw(_shapeA, _shapeB, _location, _scale, _seed, _ndraws);
            var mu = (_shapeB * _location + _shapeA * (_location + _scale)) / (_shapeA + _shapeB);
            var mean = list.Average();
            Assert.AreEqual(mu, mean, 0.3);
        }

        /// <summary>
        /// Using Numerics for Beta
        /// </summary>
        [TestMethod]
        public void BetaScaledDistribution_TestPlot() {
            var list = draw(_shapeA, _shapeB, _location, _scale, _seed, _ndraws);
            var title = "BetaScaled";
            var chartCreator = new HistogramChartCreator(list, title);
            WritePng(chartCreator, title);
        }

        private static List<double> draw(double shapeA, double shapeB, double location, double scale, int seed, int ndraws) {
            var random = new McraRandomGenerator(seed);
            var distribution = new BetaScaledDistribution(shapeA, shapeB, location, scale);
            return distribution.Draws(random, ndraws);
        }
    }
}
