using MCRA.General;
using MCRA.Simulation.Calculators.ProcessingFactorCalculation.ProcessingFactorModels;
using MCRA.Simulation.Test.Mock.FakeDataGenerators;
using MCRA.Utils.Statistics;

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

            var model = new PFLogNormalModel(pf);
            model.CalculateParameters();
            Assert.IsFalse(double.IsNaN(model?.DegreesOfFreedom ?? double.NaN));

            var nominal = model.GetNominalValue();
            Assert.AreEqual(0.6, nominal);

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
            var random = new McraRandomGenerator(1);
            var pf0 = FakeProcessingFactorsGenerator.Create(0, random);
            var model = new PFLogNormalModel(pf0);
            model.CalculateParameters();

            var draw = model.DrawFromDistribution(random);
            Assert.AreEqual(0.610, draw, 1e-3);
            Assert.AreEqual(-0.511, model.Mu, 1e-3);
            Assert.AreEqual(0.043, model.Sigma, 1e-3);

            model.Resample(random);
            Assert.AreEqual(-0.516, model.Mu, 1e-3);
            Assert.AreEqual(0.065, model.Sigma, 1e-3);
            Assert.AreEqual(2, model.DegreesOfFreedom.Value);
        }
    }
}
