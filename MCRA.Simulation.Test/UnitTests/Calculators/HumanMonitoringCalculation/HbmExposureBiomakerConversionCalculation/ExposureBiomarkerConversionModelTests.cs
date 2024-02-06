using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion;
using MCRA.Utils.Statistics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HbmExposureBiomarkerConversionCalculator {

    /// <summary>
    /// ProcessingFactorCalculation calculator
    /// </summary>
    [TestClass]
    public class ExposureBiomarkerConversionModelTests {


        [TestMethod]
        [DataRow(.5, BiomarkerConversionDistribution.Unspecified, null)]
        [DataRow(.5, BiomarkerConversionDistribution.LogNormal, .6)]
        [DataRow(.5, BiomarkerConversionDistribution.Uniform, .6)]
        public void ExposureBiomarkerConversionModel_TestsCreateAndCalculateParameters(
            double factor,
            BiomarkerConversionDistribution distribution,
            double? upper
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var conversion = new ExposureBiomarkerConversion() {
                Factor = factor,
                Distribution = distribution,
                VariabilityUpper = upper
            };
            var model = ExposureBiomarkerConversionCalculatorFactory.Create(conversion);
            var draw = model.Draw(random);
            Assert.IsTrue(draw > 0);
        }

        [TestMethod]
        public void ExposureBiomarkerConversionModel_TestsConstant() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var conversion = new ExposureBiomarkerConversion() {
                Factor = .5,
            };
            var model = new ExposureBiomarkerConversionConstantModel(conversion);
            model.CalculateParameters();
            var draw = model.Draw(random);
            Assert.IsTrue(draw > 0);
        }

        [TestMethod]
        public void ExposureBiomarkerConversionModel_TestsLognormal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var conversion = new ExposureBiomarkerConversion() {
                Factor = .5,
                VariabilityUpper = 0.6
            };
            var model = new ExposureBiomarkerConversionLogNormalModel(conversion);
            model.CalculateParameters();
            var draw = model.Draw(random);
            Assert.IsTrue(draw > 0);
        }
        
        [TestMethod]
        public void ExposureBiomarkerConversionModel_TestsUniform() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var conversion = new ExposureBiomarkerConversion() {
                Factor = .5,
                VariabilityUpper = 0.6
            };
            var model = new ExposureBiomarkerConversionUniformModel(conversion);
            model.CalculateParameters();
            var draw = model.Draw(random);
            Assert.IsTrue(draw > 0);
        }
    }
}

