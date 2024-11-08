using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.ProcessingFactorCalculation {

    /// <summary>
    /// ProcessingFactorCalculation calculator
    /// </summary>
    [TestClass]
    public class PFLogisticModelTests : PFModelTestsBase {

        /// <summary>
        /// Logistic processing factor model
        /// </summary>
        [TestMethod]
        public void PFLogisticModel_Test1() {
            var pf = mockProcessingFactor(ProcessingDistributionType.LogisticNormal, 0.6, 0.7, 0.8, 0.9);

            var model = new PFLogisticModel();
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
        /// Large variability, degrees fo freedom is 35
        /// </summary>
        [TestMethod]
        public void PFLogNormalModelTest2() {
            var model = new PFLogisticModel();
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var pf1 = FakeProcessingFactorsGenerator.Create(1, random);

            model.CalculateParameters(pf1);
            (var draw, var isCorrect) = model.DrawFromDistribution(random);

            Assert.AreEqual(0.610, draw, 1e-3);
            Assert.IsTrue(isCorrect);
            Assert.AreEqual(0.403, model.Mu, 1e-3);
            Assert.AreEqual(0.114, model.Sigma, 1e-3);

            model.Resample(random);
            Assert.AreEqual(0.391, model.Mu, 1e-3);
            Assert.AreEqual(0.249, model.Sigma, 1e-3);
            Assert.AreEqual(1, model.DegreesOfFreedom.Value);
        }
    }
}
