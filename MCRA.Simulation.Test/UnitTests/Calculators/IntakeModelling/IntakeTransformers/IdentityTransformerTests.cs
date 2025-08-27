using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.Test.UnitTests.IntakeModelling {
    /// <summary>
    /// IntakeModelling calculator
    /// </summary>
    [TestClass]
    public class IdentityTransformerTests {
        /// <summary>
        /// Transformer: identity
        /// </summary>
        [TestMethod]
        public void IdentityTransformer_Test() {
            var x = 10d;
            var transformer = new IdentityTransformer();
            var resultTransform = transformer.Transform(x);
            var resultBackTransform = transformer.InverseTransform(x);
            Assert.AreEqual(resultTransform, x);
            Assert.AreEqual(resultBackTransform, x);
        }

        /// <summary>
        /// Gauss Hermite transformer: identity
        /// </summary>
        [TestMethod]
        public void IdentityTransformer_TestBiasCorrectedInverseTransform() {
            var x = 10d;
            var transformer = new IdentityTransformer();
            var resultInverseTransform = transformer.BiasCorrectedInverseTransform(x, 3);
            Assert.AreEqual(resultInverseTransform, x);
        }
    }
}
