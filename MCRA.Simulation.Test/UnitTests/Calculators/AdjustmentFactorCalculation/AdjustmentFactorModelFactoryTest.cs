using MCRA.Utils.Statistics;
using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.AdjustmentFactorCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.AdjustmentFactorCalculation {

    /// <summary>
    ///  AdjustmentFactorModelFactory calculator
    /// </summary>
    [TestClass]
    public class AdjustmentFactorModelFactoryTests {

        /// <summary>
        /// Test AdjustmentFactorModelFactory.
        /// </summary>
        [TestMethod]
        public void AdjustmentFactorModelFactory_TestCreateNone() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var A = 0d;
            var B = 0d;
            var C = 0d;
            var D = 0d;
            var settings = new AdjustmentFactorModelFactorySettings(new EffectModelSettings() {
                    ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.None,
                    ExposureParameterA = A,
                    ExposureParameterB = B,
                    ExposureParameterC = C,
                    ExposureParameterD = D,
                }, isExposure : true
            );
            var model = new AdjustmentFactorModelFactory(settings);
            var calculator = model.Create();
            Assert.IsTrue(calculator is AFFixedModel);
            var factor = calculator.GetNominal();
            Assert.AreEqual(1, factor);
            var factorUnc = calculator.DrawFromDistribution(random);
            Assert.AreEqual(factorUnc, 1);
        }

        /// <summary>
        /// Test AdjustmentFactorModelFactory.
        /// </summary>
        [TestMethod]
        public void AdjustmentFactorModelFactory_TestCreateFixed() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var A = 2d;
            var B = 3d;
            var C = 1d;
            var D = 1d;
            var settings = new AdjustmentFactorModelFactorySettings(new EffectModelSettings() {
                    ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Fixed,
                    ExposureParameterA = A,
                    ExposureParameterB = B,
                    ExposureParameterC = C,
                    ExposureParameterD = D,
                }, isExposure: true
            );
            var model = new AdjustmentFactorModelFactory(settings);
            var calculator = model.Create();
            Assert.IsTrue(calculator is AFFixedModel);
            var factor = calculator.GetNominal();
            Assert.AreEqual(A, factor);
            var factorUnc = calculator.DrawFromDistribution(random);
            Assert.AreEqual(factorUnc, A);
        }

        /// <summary>
        /// Test AdjustmentFactorModelFactory .
        /// </summary>
        [TestMethod]
        public void AdjustmentFactorModelFactory_TestCreateBeta() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var A = 2d;
            var B = 3d;
            var C = 0d;
            var D = 1d;
            var settings = new AdjustmentFactorModelFactorySettings(new EffectModelSettings() {
                    ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Beta,
                    ExposureParameterA = A,
                    ExposureParameterB = B,
                    ExposureParameterC = C,
                    ExposureParameterD = D,
                }, isExposure: true
            );
            var model = new AdjustmentFactorModelFactory(settings);
            var calculator = model.Create();
            Assert.IsTrue(calculator is AFBetaModel);
            var factor = calculator.GetNominal();
            var factorUnc = calculator.DrawFromDistribution(random);
        }

        /// <summary>
        /// Test AdjustmentFactorModelFactory .
        /// </summary>
        [TestMethod]
        public void AdjustmentFactorModelFactory_TestCreateGamma() {
            var A = 2d;
            var B = 3d;
            var C = 1d;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var settings = new AdjustmentFactorModelFactorySettings(new EffectModelSettings() {
                    ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Gamma,
                    ExposureParameterA = A,
                    ExposureParameterB = B,
                    ExposureParameterC = C,
                }, isExposure: true
            );
            var model = new AdjustmentFactorModelFactory(settings);
            var calculator = model.Create();
            Assert.IsTrue(calculator is AFGammaModel);
            var factor = calculator.GetNominal();
            var factorUnc = calculator.DrawFromDistribution(random);
        }

        /// <summary>
        /// Test AdjustmentFactorModelFactory .
        /// </summary>
        [TestMethod]
        public void AdjustmentFactorModelFactory_TestCreateLogNormal() {
            var A = 2d;
            var B = 3d;
            var C = 1d;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var settings = new AdjustmentFactorModelFactorySettings(new EffectModelSettings() {
                    ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.LogNormal,
                    ExposureParameterA = A,
                    ExposureParameterB = B,
                    ExposureParameterC = C,
                }, isExposure: true
            );
            var model = new AdjustmentFactorModelFactory(settings);
            var calculator = model.Create();
            Assert.IsTrue(calculator is AFLognormalModel);
            var factor = calculator.GetNominal();
            Assert.AreEqual(C + Math.Exp(A), factor);
            var factorUnc = calculator.DrawFromDistribution(random);
        }

        /// <summary>
        /// Test AdjustmentFactorModelFactory .
        /// </summary>
        [TestMethod]
        public void AdjustmentFactorModelFactory_TestCreateLogStudent_t() {
            var A = 2d;
            var B = 3d;
            var C = 0.99d;
            var D = 0d;
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var settings = new AdjustmentFactorModelFactorySettings(new EffectModelSettings() {
                    ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.LogStudents_t,
                    ExposureParameterA = A,
                    ExposureParameterB = B,
                    ExposureParameterC = C,
                    ExposureParameterD = D,
                }, isExposure: true
            );
            var model = new AdjustmentFactorModelFactory(settings);
            var calculator = model.Create();
            Assert.IsTrue(calculator is AFLogStudentTModel);
            var factor = calculator.GetNominal();
            Assert.AreEqual(Math.Exp(A) + D, factor);
            var factorUnc = calculator.DrawFromDistribution(random);
        }
    }
}