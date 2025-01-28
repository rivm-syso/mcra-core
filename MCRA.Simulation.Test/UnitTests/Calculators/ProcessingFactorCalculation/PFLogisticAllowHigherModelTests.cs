using MCRA.General;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ProcessingFactorCalculation {
    /// <summary>
    /// ProcessingFactorCalculation calculator
    /// </summary>
    [TestClass]
    public class PFLogisticAllowHigherModelTests : PFModelTestsBase {
        /// <summary>
        /// Logistic processing factor model, allow higher than 1
        /// </summary>
        [TestMethod]
        public void PFLogisticAllowHigherModel_Tests() {
            var pf = mockProcessingFactor(ProcessingDistributionType.LogNormal, 0.6, 0.7, 0.8, 0.9);

            var model = new PFLogisticAllowHigherModel(pf);
            model.CalculateParameters();

            var nominal = model.GetNominalValue();
            Assert.AreEqual(1, nominal);

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var draw = model.DrawFromDistribution(random);
            Assert.AreEqual(1, draw);

            var n = 500;
            var samples = runUncertains(model, random, n);
            var median = samples.Median();
            Assert.AreEqual(1, median, 1e-1);
        }
    }
}
