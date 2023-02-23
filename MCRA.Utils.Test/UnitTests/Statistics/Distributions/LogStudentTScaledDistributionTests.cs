using MCRA.Utils.Charting.OxyPlot;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {
    [TestClass]
    public class LogStudentTScaledDistributionTests : DistributionsTestsBase {

        private int _seed = 1;
        private int _ndraws = 10000;
        private double _location = 0.593;
        private double _scale = .367;
        private double _freedom = 5;
        private double _offset = 10d;

        /// <summary>
        /// Using Numerics for LogStudentTScaled
        /// </summary>
        [TestMethod]
        public void LogStudentTScaledDistribution_TestDrawN() {
            var list = draw(_location, _scale, _freedom, _seed, _ndraws, _offset);
            var mu = Math.Exp(_location + Math.Pow(_scale, 2) / 2) + _offset;
            var mean = list.Average();
            Assert.AreEqual(mu, mean, 0.3);
        }

        /// <summary>
        /// Using Numerics for Beta
        /// </summary>
        [TestMethod]
        public void LogStudentTScaledDistribution_TestPlot() {
            var list = draw(_location, _scale, _freedom, _seed, _ndraws, _offset);
            var title = "LogStudentTScaled";
            var chartCreator = new HistogramChartCreator(list, title);
            WritePng(chartCreator, title);
        }

        private static List<double> draw(double location, double scale, double freedom, int seed, int ndraws, double offset = 0) {
            var random = new McraRandomGenerator(seed);
            var distribution = new LogStudentTScaledDistribution(location, scale, freedom, offset);
            return distribution.Draws(random, ndraws);
        }
    }
}
