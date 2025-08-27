using MCRA.Utils;
using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.IntakeModelling;

namespace MCRA.Simulation.Test.UnitTests.IntakeModelling.IntakeTransformers {
    /// <summary>
    /// IntakeModelling calculator
    /// </summary>
    [TestClass]
    public class PowerTransformerTests {

        /// <summary>
        /// Transformer: power, Gauss Hermite
        /// </summary>
        [TestMethod]
        public void PowerTransformer_TestTransform() {
            var x = 10d;
            var transformer = new PowerTransformer();
            var power = 0.2;
            transformer.Power = power;
            transformer.GaussHermitePoints = UtilityFunctions.GaussHermite(UtilityFunctions.GaussHermitePoints);

            var result1 = transformer.Transform(x);
            var result2 = transformer.InverseTransform(result1);
            Assert.AreEqual(result1, ((Math.Pow(x, power) - 1) / power), 1e-5);
            Assert.AreEqual(result2, x, 1e-5);
        }

        /// <summary>
        /// Transformer: power
        /// </summary>
        [TestMethod]
        public void PowerTransformer_TestCalculatePower() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var logNormal = new LogNormalDistribution(0, 1);
            var y = new List<double>();
            for (int i = 0; i < 100; i++) {
                y.Add(logNormal.Draw(random));
            }
            var power = PowerTransformer.CalculatePower(y);
            Assert.AreEqual(0.046, power, 1e-3);
        }

        /// <summary>
        /// Gauss Hermite transformer: power
        /// </summary>
        [TestMethod]
        public void PowerTransformer_TestBiasCorrectedInverseTransform1() {
            var x = 10d;
            var gaussHermitePoints = UtilityFunctions.GaussHermite(UtilityFunctions.GaussHermitePoints);
            var transformer = new PowerTransformer(0.2, gaussHermitePoints);
            var result1 = transformer.BiasCorrectedInverseTransform(x, 3d);
            Assert.AreEqual(276.048, result1, 1e-3);
        }

        /// <summary>
        /// Transformer: power, Gauss Hermite
        /// </summary>
        [TestMethod]
        public void PowerTransformer_TestBiasCorrectedInverseTransform2() {
            var x = 10d;
            var power = 0.2;
            var resultTransformed = (Math.Pow(x, power) - 1) / power;
            var gaussHermitePoints = UtilityFunctions.GaussHermite(UtilityFunctions.GaussHermitePoints);
            var ghTransformer = new PowerTransformer(power, gaussHermitePoints);
            var resultInverseTransform = ghTransformer.BiasCorrectedInverseTransform(resultTransformed, 3d);
            Assert.AreEqual(15.119, resultInverseTransform, 1e-3);
        }
    }
}
