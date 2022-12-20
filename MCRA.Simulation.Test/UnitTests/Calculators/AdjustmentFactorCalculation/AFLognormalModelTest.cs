using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.AdjustmentFactorCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MCRA.Simulation.Test.UnitTests.Calculators.AdjustmentFactorCalculation {

    /// <summary>
    /// AFLognormalModel
    /// </summary>
    [TestClass]
    public class AFLognormalModelTest {

        /// <summary>
        /// Lognormal adjustement factors
        /// </summary>
        [TestMethod]
        public void AFLognormalModel_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var A = 2d;
            var B = 3d;
            var C = 1d;
            var model = new AFLognormalModel(A, B, C);
            var factor = model.GetNominal();
            Assert.AreEqual(C + Math.Exp(A), factor);
            var factorUnc = model.DrawFromDistribution(random);
        }
    }
}
