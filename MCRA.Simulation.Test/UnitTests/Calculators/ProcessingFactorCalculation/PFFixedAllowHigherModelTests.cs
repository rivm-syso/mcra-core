using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ProcessingFactorCalculation {

    /// <summary>
    /// ProcessingFactorCalculation calculator
    /// </summary>
    [TestClass]
    public class PFFixedAllowHigherModelTests : PFModelTestsBase {

        /// <summary>
        /// Fixed processing factors, allow higher than 1
        /// </summary>
        [TestMethod]
        public void PFFixedAllowHigherModel_Test1() {
            var pf = mockProcessingFactor(ProcessingDistributionType.LogNormal, 0.6, 0.7, 0.8, 0.9);

            var model = new PFFixedAllowHigherModel();
            model.CalculateParameters(pf);

            (var nominal, var isCorrectNominal) = model.GetNominalValue();
            Assert.AreEqual(1D, nominal);
            Assert.IsFalse(isCorrectNominal);

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            (var draw, var isCorrectDraw) = model.DrawFromDistribution(random);
            Assert.AreEqual(1D, draw);
            Assert.IsFalse(isCorrectDraw);

            var n = 500;
            var samples = runUncertains(model, random, n);
            Assert.IsTrue(samples.Median() >= 1 );
        }
    }
}
