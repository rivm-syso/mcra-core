using MCRA.General;
using MCRA.Simulation.Calculators.AdjustmentFactorCalculation;
using MCRA.Utils.Statistics;

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
            var settings = new AdjustmentFactorModelFactorySettings(new() {
                ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.None,
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
            Assert.AreEqual(1, factor);
            var factorUnc = calculator.DrawFromDistribution(random);
            Assert.AreEqual(1, factorUnc);
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
            var settings = new AdjustmentFactorModelFactorySettings(new() {
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
            var settings = new AdjustmentFactorModelFactorySettings(new() {
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
            var settings = new AdjustmentFactorModelFactorySettings(new() {
                ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Gamma,
                ExposureParameterA = A,
                ExposureParameterB = B,
                ExposureParameterC = C,
            }, isExposure: true
            );
            var model = new AdjustmentFactorModelFactory(settings);
            var calculator = getGamma(A, B, C);
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
            var settings = new AdjustmentFactorModelFactorySettings(new() {
                ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.LogNormal,
                ExposureParameterA = A,
                ExposureParameterB = B,
                ExposureParameterC = C,
            }, isExposure: true
            );
            var model = new AdjustmentFactorModelFactory(settings);
            var calculator = getLogNormal(A, B, C);
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
            var settings = new AdjustmentFactorModelFactorySettings(new() {
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

        /// <summary>
        /// Test whether the fixed approach is more or less similar to the distribution based approach.
        /// e.g. fixed > distribution should be around 50%
        /// </summary>
        [TestMethod]
        public void AdjustmentFactorModelFactory_TestSimulationGamma() {
            var medianFixed = new List<double>();
            var medianDistribution = new List<double>();
            var seed = 12345;
            var random = new McraRandomGenerator(seed);
            var nSimu = 1000;
            var logNormal = getLogNormal(0.3, .4, .1);
            var gamma1 = getGamma(3.84, 3.28, 0.5);
            var gamma2 = getGamma(2.13, 2.24, 0.3);
            var nominal1 = gamma1.GetNominal();
            var nominal2 = gamma2.GetNominal();
            //Repeat simulation
            for (var j = 0; j < nSimu; j++) {
                var simuFixed = new List<double>();
                var simuDistribution = new List<double>();
                //Uncertainty runs
                for (var i = 0; i < 100; i++) {
                    var ratioHazardExposure = logNormal.DrawFromDistribution(random);
                    simuFixed.Add(ratioHazardExposure * nominal1 * nominal2);
                    simuDistribution.Add(ratioHazardExposure * gamma1.DrawFromDistribution(random) * gamma2.DrawFromDistribution(random));
                }
                //Calculate mean and median of fixed adjustment and distribution based adjustement
                medianFixed.Add(simuFixed.Median());
                medianDistribution.Add(simuDistribution.Median());
            }
            var sumMedian = 0;
            for (var j = 0; j < nSimu; j++) {
                sumMedian += (medianFixed[j] > medianDistribution[j]) ? 1 : 0;
            }
        }

        private static AdjustmentFactorModelBase getGamma(double A, double B, double C) {
            var settings = new AdjustmentFactorModelFactorySettings(new () {
                ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.Gamma,
                ExposureParameterA = A,
                ExposureParameterB = B,
                ExposureParameterC = C,
            }, isExposure: true
                            );
            var model = new AdjustmentFactorModelFactory(settings);
            var calculator = model.Create();
            return calculator;
        }


        private static AdjustmentFactorModelBase getLogNormal(double A, double B, double C) {
            var settings = new AdjustmentFactorModelFactorySettings(new () {
                ExposureAdjustmentFactorDistributionMethod = AdjustmentFactorDistributionMethod.LogNormal,
                ExposureParameterA = A,
                ExposureParameterB = B,
                ExposureParameterC = C,
            }, isExposure: true
                        );
            var model = new AdjustmentFactorModelFactory(settings);
            var calculator = model.Create();
            return calculator;
        }
    }
}