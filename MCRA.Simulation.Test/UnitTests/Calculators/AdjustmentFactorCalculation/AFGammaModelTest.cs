using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.AdjustmentFactorCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MCRA.Simulation.Test.UnitTests.Calculators.AdjustmentFactorCalculation {

    /// <summary>
    /// AFGammaModel
    /// </summary>
    [TestClass]
    public class AFGammaModelTest {

        /// <summary>
        /// Gamma adjustement factors
        /// </summary>
        [TestMethod]
        public void AFGammaModel_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var A = 2d;
            var B = 3d;
            var C = 1d;
            var model = new AFGammaModel(A, B, C);
            var factor = model.GetNominal();
            var factorUnc = model.DrawFromDistribution(random);
        }
    }
}
