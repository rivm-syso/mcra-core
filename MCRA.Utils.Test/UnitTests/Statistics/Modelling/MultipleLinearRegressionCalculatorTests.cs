using MCRA.Utils.Statistics.Modelling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class MultipleLinearRegressionCalculatorTests {

        [TestMethod]
        public void MultipleRegression_Test1() {
            var x = new double[,] { { 1, 0, 1 }, { 1, 0, 2 }, { 1, 0, 1 }, { 1, 0, 2 }, { 1, 1, 1 }, { 1, 1, 2 }, { 1, 1, 1 }, { 1, 1, 2 } };
            var y = new List<double> { 3, 5, 3, 5, 4, 6, 4, 6 };
            var w = new List<double> { 1, 1, 1, 1, 1, 1, 1, 1 };
            var mlrResult = MultipleLinearRegressionCalculator.Compute(x, y, w);
            Assert.AreEqual(1, mlrResult.RegressionCoefficients[0], 1e-6);
            Assert.AreEqual(1, mlrResult.RegressionCoefficients[1], 1e-6);
            Assert.AreEqual(2, mlrResult.RegressionCoefficients[2], 1e-6);
            Assert.IsTrue(mlrResult.DegreesOfFreedom == 5);
        }
    }
}
