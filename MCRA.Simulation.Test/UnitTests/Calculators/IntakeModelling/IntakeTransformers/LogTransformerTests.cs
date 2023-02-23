using MCRA.Simulation.Calculators.IntakeModelling;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.IntakeModelling {
    /// <summary>
    /// IntakeModelling calculator
    /// </summary>
    [TestClass]
    public class LogTransformerTests {

        /// <summary>
        /// Transformer: logarithmic
        /// </summary>
        [TestMethod]
        public void LogTransformerTests_Test() {
            var x = 10d;
            var transformer = new LogTransformer();
            var result1 = transformer.Transform(x);
            var result2 = transformer.InverseTransform(result1);
            Assert.AreEqual(result1, Math.Log(x), 1e-5);
            Assert.AreEqual(result2, x, 1e-5);
        }

        /// <summary>
        /// Gauss Hermite transformer: logarithmic
        /// </summary>
        [TestMethod]
        public void GHLogTransformer_TestBiasCorrectedInverseTransform1() {
            var transformer = new LogTransformer();
            var result1 = transformer.BiasCorrectedInverseTransform(.3, .1);
            Assert.AreEqual(1.419, result1, 1e-3);
        }

        /// <summary>
        /// Transformer: logarithmic
        /// </summary>
        [TestMethod]
        public void LogTransformerTests_TestBiasCorrectedInverseTransform2() {
            var x = 10d;
            var varianceWithin = 3d;
            var resultTransform = Math.Log(x);
            var transformer = new LogTransformer();
            var resultInverseTransform = transformer.BiasCorrectedInverseTransform(resultTransform, varianceWithin);
            var test = Math.Exp(resultTransform + varianceWithin / 2);
            Assert.AreEqual(resultInverseTransform, test, 1e-5);
        }
    }
}
