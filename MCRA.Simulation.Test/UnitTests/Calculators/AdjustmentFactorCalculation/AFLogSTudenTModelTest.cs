using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.AdjustmentFactorCalculation;

namespace MCRA.Simulation.Test.UnitTests.Calculators.AdjustmentFactorCalculation {

    /// <summary>
    /// AFLogStudentTModel
    /// </summary>
    [TestClass]
    public class AFLogStudentModelTest {

        /// <summary>
        /// LogStudentT adjustement factors
        /// </summary>
        [TestMethod]
        public void AFLogStudentTModel_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var A = 2d;
            var B = 3d;
            var C = 0.99d;
            var D = 0d;
            var model = new AFLogStudentTModel(A, B, C, D);
            var factor = model.GetNominal();
            Assert.AreEqual(Math.Exp(A) + D, factor);
            var factorUnc = model.DrawFromDistribution(random);
        }
    }
}
