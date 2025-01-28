using MCRA.General;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ProcessingFactorCalculation {

    /// <summary>
    /// ProcessingFactorCalculation calculator
    /// </summary>
    [TestClass]
    public class PFFixedAllowHigherModelTests : PFModelTestsBase {

        /// <summary>
        /// Fixed processing factors, allow higher than 1.
        /// </summary>
        [TestMethod]
        public void PFFixedAllowHigherModel_TestDraw() {
            var pf = mockProcessingFactor(ProcessingDistributionType.LogNormal, 0.6, 0.7, 0.8, 0.9);

            var model = new PFFixedAllowHigherModel(pf);
            model.CalculateParameters();

            var nominal = model.GetNominalValue();
            Assert.AreEqual(1D, nominal);

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var draw = model.DrawFromDistribution(random);
            Assert.AreEqual(1D, draw);

            var n = 500;
            var samples = runUncertains(model, random, n);
            Assert.IsTrue(samples.Median() >= 1 );
        }
    }
}
