using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.HbmExposureBiomarkerConversion.ExposureBiomarkerConversionModels;
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
        [DataRow(.3, BiomarkerConversionDistribution.Beta, 0.06)]
        public void ExposureBiomarkerConversionModel_TestsCreateAndCalculateParameters(
            double factor,
            BiomarkerConversionDistribution distribution,
            double? upper
        ) {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var conversion = new ExposureBiomarkerConversion() {
                ConversionFactor = factor,
                Distribution = distribution,
                VariabilityUpper = upper
            };
            var model = ExposureBiomarkerConversionCalculatorFactory.Create(conversion, false);
            var draw = model.Draw(random, null, GenderType.Undefined);
            Assert.IsTrue(draw > 0);
        }

        [TestMethod]
        public void ExposureBiomarkerConversionModel_TestsConstant() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var conversion = new ExposureBiomarkerConversion() {
                ConversionFactor = .5,
            };
            var model = new ExposureBiomarkerConversionConstantModel(conversion, false);
            model.CalculateParameters();
            var draw = model.Draw(random, null, GenderType.Undefined);
            Assert.IsTrue(draw > 0);
        }

        [TestMethod]
        public void ExposureBiomarkerConversionModel_TestsLognormal() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var conversion = new ExposureBiomarkerConversion() {
                ConversionFactor = .5,
                VariabilityUpper = 0.6
            };
            var model = new ExposureBiomarkerConversionLogNormalModel(conversion, false);
            model.CalculateParameters();
            var draw = model.Draw(random, null, GenderType.Undefined);
            Assert.IsTrue(draw > 0);
        }

        [TestMethod]
        public void ExposureBiomarkerConversionModel_TestsUniform() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var conversion = new ExposureBiomarkerConversion() {
                ConversionFactor = .5,
                VariabilityUpper = 0.6
            };
            var model = new ExposureBiomarkerConversionUniformModel(conversion, false);
            model.CalculateParameters();
            var draw = model.Draw(random, null, GenderType.Undefined);
            Assert.IsTrue(draw > 0);
        }

        [TestMethod]
        public void ExposureBiomarkerConversionModel_TestsBeta() {
            var seed = 1;
            var random = new McraRandomGenerator(seed);
            var conversion = new ExposureBiomarkerConversion() {
                ConversionFactor = .3,
                VariabilityUpper = 0.06
            };
            var model = new ExposureBiomarkerConversionBetaModel(conversion, false);
            model.CalculateParameters();
            var draw = model.Draw(random, null, GenderType.Undefined);
            Assert.IsTrue(draw > 0);
        }
    }
}
