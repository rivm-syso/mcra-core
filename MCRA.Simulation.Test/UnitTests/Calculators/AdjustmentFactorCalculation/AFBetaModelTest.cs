using MCRA.Utils.Statistics;
using MCRA.Simulation.Calculators.AdjustmentFactorCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace MCRA.Simulation.Test.UnitTests.Calculators.AdjustmentFactorCalculation {

    /// <summary>
    /// AFBetaModel
    /// </summary>
    [TestClass]
    public class AFBetaModelTest {

        /// <summary>
        /// Beta adjustement factors
        /// </summary>
        [TestMethod]
        public void AFBetaModel_Test1() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var A = 2d;
            var B = 3d;
            var C = 0d;
            var D = 1.01d;
            var model = new AFBetaModel(A, B, C, D);
            var factor = model.GetNominal();
            var factorUnc = model.DrawFromDistribution(random);
        }
    }
}
