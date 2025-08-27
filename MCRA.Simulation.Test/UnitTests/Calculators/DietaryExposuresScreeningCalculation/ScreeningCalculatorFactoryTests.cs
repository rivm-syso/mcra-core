using MCRA.General;
using MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinations;
using MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinationsCalculation;

namespace MCRA.Simulation.Test.UnitTests.Calculators.DietaryExposuresScreeningCalculation {

    /// <summary>
    /// ScreeningCalculatorFactory tests.
    /// </summary>
    [TestClass]
    public class ScreeningCalculatorFactoryTests {

        /// <summary>
        /// ScreeningCalculatorFactory: acute
        /// </summary>
        [TestMethod]
        public void ScreeningCalculatorFactory_TestsCreateAcute() {
            var settings = new ScreeningCalculatorFactorySettings(new() {
                CriticalExposurePercentage = 95,
                CumulativeSelectionPercentage = 95,
                ImportanceLor = 0,
                ExposureType = ExposureType.Acute
            });
            var calculator = new ScreeningCalculatorFactory(settings, isPerPerson: false);
            calculator.Create();
            Assert.IsNotNull(calculator);
        }

        /// <summary>
        /// ScreeningCalculatorFactory: chronic
        /// </summary>
        [TestMethod]
        public void ScreeningCalculatorFactory_TestsChronic() {
            var settings = new ScreeningCalculatorFactorySettings(new() {
                CriticalExposurePercentage = 95,
                CumulativeSelectionPercentage = 95,
                ImportanceLor = 0,
                ExposureType = ExposureType.Chronic
            });
            var calculator = new ScreeningCalculatorFactory(settings, isPerPerson: false);
            calculator.Create();
            Assert.IsNotNull(calculator);
        }
    }
}
