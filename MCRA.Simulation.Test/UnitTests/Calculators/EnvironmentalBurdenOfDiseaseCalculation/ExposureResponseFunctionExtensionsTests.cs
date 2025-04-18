using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    [TestClass]
    public class ExposureResponseFunctionExtensionsTests {
        [TestMethod]
        public void ExposureResponseFunctionExtensions_TestCompute() {
            var erf = new ExposureResponseFunction() {
                ExposureResponseType = ExposureResponseType.Constant,
                ExposureResponseSpecification = new NCalc.Expression("5.01")
            };
            var result = erf.Compute(42);
            Assert.AreEqual(5.01, result);
        }
    }
}
