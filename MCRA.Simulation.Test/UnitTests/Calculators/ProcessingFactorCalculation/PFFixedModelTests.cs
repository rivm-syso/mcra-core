using MCRA.General;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ProcessingFactorCalculation {

    /// <summary>
    /// ProcessingFactorCalculation calculator
    /// </summary>
    [TestClass]
    public class PFFixedModelTests : PFModelTestsBase {

        /// <summary>
        /// Fixed processing factors
        /// </summary>
        [TestMethod]
        public void PFFixedModel_Test1() {
            var pf = mockProcessingFactor(ProcessingDistributionType.LogNormal, 0.6, 0.7, 0.8, 0.9);

            var model = new PFFixedModel(pf);
            model.CalculateParameters();

            var nominal = model.GetNominalValue();
            Assert.AreEqual(0.6, nominal);

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var draw = model.DrawFromDistribution(random);
            Assert.AreEqual(0.6, draw);

            var n = 500;
            var samples = runUncertains(model, random, n);
            Assert.AreEqual(0.6, samples.Median(), 1e-1);
        }
    }
}
