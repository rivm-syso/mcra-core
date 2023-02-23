using MCRA.Utils.Statistics;
using MCRA.Utils.Test.UnitTests.Helpers.ChartCreators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests.Statistics.Distributions {

    /// <summary>
    /// Tests the multivariate normal distribution function.
    /// </summary>
    [TestClass()]
    public class MultiVariateNormalDistributionTests : DistributionsTestsBase {

        [TestMethod]
        public void MultiVariateNormalDistribution_Test() {
            var seed = 1;
            var mu1 = .4;
            var mu2 = .5;
            var numberOfIterations = 10000;
            var covar = .8;
            var var1 = .2;
            var var2 = .3;
            var mean = new List<double> { mu1, mu2 };
            var chol = new double[,] { { var1, covar }, { covar, var2 } };
            var random = new McraRandomGenerator(seed);

            var rn1 = new List<double>();
            var rn2 = new List<double>();
            var draw = MultiVariateNormalDistribution.Draw(mean, chol, numberOfIterations, random);
            for (int i = 0; i < numberOfIterations; i++) {
                rn1.Add(draw[i, 0]);
                rn2.Add(draw[i, 1]);
            }

            var chart = new ScatterChartCreator(rn1, rn2);
            WritePng(chart, "MultiVariateNormalDistribution1");

            var mn1 = new List<double>();
            var mn2 = new List<double>();
            var result = MultiVariateNormalDistribution.Draw(mean, chol, numberOfIterations, random);
            for (int i = 0; i < numberOfIterations; i++) {
                mn1.Add(result[i, 0]);
                mn2.Add(result[i, 1]);
            }

            var mn1Mean = mn1.Average();
            var mn2Mean = mn2.Average();

            Assert.AreEqual(mu1, mn1Mean, 1e-2);
            Assert.AreEqual(mu2, mn2Mean, 1e-1);

            var chart1 = new ScatterChartCreator(mn1, mn2);
            WritePng(chart1, "MultiVariateNormalDistribution2");
        }
    }
}
