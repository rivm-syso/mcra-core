using MCRA.Utils.Statistics.Modelling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Utils.Test.UnitTests {
    [TestClass]
    public class SimpleLinearRegressionCalculatorTests {

        [TestMethod]
        public void SimpleLinearRegression_Test1() {
            var x = new List<double>();
            var y = new List<double>();
            try {
                var result = SimpleLinearRegressionCalculator.Compute(x, y);
            } catch (ParameterFitException) {
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void SimpleLinearRegression_Test2() {
            var x = new List<double>() { 0 };
            var y = new List<double>() { 100 };
            try {
                var result = SimpleLinearRegressionCalculator.Compute(x, y);
            } catch (ParameterFitException) {
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void SimpleLinearRegression_Test3() {
            var x = new List<double>() { 0, 1 };
            var y = new List<double>() { 1, 3 };
            var result = SimpleLinearRegressionCalculator.Compute(x, y);
            Assert.IsTrue(double.IsNaN(result.MeanDeviance));
            Assert.AreEqual(2, result.Coefficient, 1e-6);
            Assert.AreEqual(1, result.Constant, 1e-6);
        }

        [TestMethod]
        public void SimpleLinearRegression_Test4() {
            var x = new List<double>() { 0, 1, 2 };
            var y = new List<double>() { 1, 3, 5 };
            var result = SimpleLinearRegressionCalculator.Compute(x, y);
            Assert.AreEqual(0, result.MeanDeviance, 1e-6);
            Assert.AreEqual(2, result.Coefficient, 1e-6);
            Assert.AreEqual(1, result.Constant, 1e-6);
        }

        [TestMethod]
        public void SimpleLinearRegression_Test5() {
            var x = new List<double>() { 0, 1, 2, -1 };
            var y = new List<double>() { 1, 3, 5, -1 };
            var result = SimpleLinearRegressionCalculator.Compute(x, y);
            Assert.AreEqual(0, result.MeanDeviance, 1e-6);
            Assert.AreEqual(2, result.Coefficient, 1e-6);
            Assert.AreEqual(1, result.Constant, 1e-6);
        }

        [TestMethod]
        public void SimpleLinearRegression6() {
            var x = new List<double> { 0, 10 };
            var y = new List<double> { 0, 10 };
            var slrResult = SimpleLinearRegressionCalculator.Compute(x, y);
            Assert.AreEqual(0, slrResult.Constant);
            Assert.AreEqual(1, slrResult.Coefficient);
        }
    }
}
