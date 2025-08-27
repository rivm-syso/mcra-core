using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.AdjustmentFactorCalculation;

namespace MCRA.Simulation.Test.UnitTests.Calculators.AdjustmentFactorCalculation {

    /// <summary>
    /// AFFixedModel
    /// </summary>
    [TestClass]
    public class AFFixedModelTest {

        /// <summary>
        /// Fixed adjustement factors
        /// </summary>
        [TestMethod]
        public void AFFixedModel_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var A = 2d;
            var model = new AFFixedModel(A);
            var factor = model.GetNominal();
            Assert.AreEqual(A, factor);
            var factorUnc = model.DrawFromDistribution(random);
        }
    }
}
