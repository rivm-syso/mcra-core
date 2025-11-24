using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.CounterFactualValueModels;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;

namespace MCRA.Simulation.Test.UnitTests.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    [TestClass]
    public class ExposureResponseFunctionExtensionsTests {
        [TestMethod]
        public void ExposureResponseFunctionExtensions_TestCompute() {
            var erf = new ExposureResponseFunction() {
                ExposureResponseType = ExposureResponseType.Constant,
                ExposureResponseSpecification = new NCalc.Expression("5.01")
            };
            var erfModel = new ExposureResponseFunctionModel(erf);
            erfModel.CounterFactualValueModel= CounterFactualValueCalculatorFactory.Create(erf);
            var result = erfModel.Compute(42, true);
            Assert.AreEqual(5.01, result);
        }
    }
}
