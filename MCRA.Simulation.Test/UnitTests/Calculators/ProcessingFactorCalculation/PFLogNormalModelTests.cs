using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Test.Mock.MockDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ProcessingFactorCalculation {

    /// <summary>
    /// ProcessingFactorCalculation calculator
    /// </summary>
    [TestClass]
    public class PFLogNormalModelTests : PFModelTestsBase {

        /// <summary>
        /// Lognormal processing factor model
        /// </summary>
        [TestMethod]
        public void PFLogNormalModel_Test1() {
            var pf = mockProcessingFactor(ProcessingDistributionType.LogNormal, 0.6, 0.7, 0.8, 0.9);

            var model = new PFLogNormalModel();
            model.CalculateParameters(pf);
            Assert.IsTrue(!double.IsNaN(model?.DegreesOfFreedom ?? double.NaN));

            (var nominal, var isCorrectNominal) = model.GetNominalValue();
            Assert.AreEqual(0.6, nominal);
            Assert.IsTrue(isCorrectNominal);

            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var samples = drawVariability(model, random, 500);
            Assert.AreEqual(0.6, samples.Median(), 1e-1);

            var n = 500;
            samples = runUncertains(model, random, n);
            Assert.AreEqual(0.6, samples.Median(), 1e-1);
        }

        /// <summary>
        /// Large variability, degrees of freedom is 79
        /// </summary>
        [TestMethod]
        public void PFLogNormalModelTest2() {
            var model = new PFLogNormalModel();
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var pf0 = MockProcessingFactorsGenerator.Create(0, random);

            model.CalculateParameters(pf0);
            (var draw, var isCorrect) = model.DrawFromDistribution(random);

            Assert.AreEqual(0.610, draw, 1e-3);
            Assert.IsTrue(isCorrect);
            Assert.AreEqual(-0.511, model.Mu, 1e-3);
            Assert.AreEqual(0.043, model.Sigma, 1e-3);

            model.Resample(random);
            Assert.AreEqual(-0.516, model.Mu, 1e-3);
            Assert.AreEqual(0.065, model.Sigma, 1e-3);
            Assert.AreEqual(2, model.DegreesOfFreedom.Value);
        }
    }
}
