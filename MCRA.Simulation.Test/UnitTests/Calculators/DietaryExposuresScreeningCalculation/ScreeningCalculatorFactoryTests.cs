using MCRA.General;
using MCRA.General.Action.Settings;
using MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinations;
using MCRA.Simulation.Calculators.HighExposureFoodSubstanceCombinationsCalculation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
            var settings = new ScreeningCalculatorFactorySettings(new ScreeningSettings() {
                CriticalExposurePercentage = 95,
                CumulativeSelectionPercentage = 95,
                ImportanceLor = 0
            },
            new AssessmentSettings() {
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
            var settings = new ScreeningCalculatorFactorySettings(new ScreeningSettings() {
                CriticalExposurePercentage = 95,
                CumulativeSelectionPercentage = 95,
                ImportanceLor = 0
            },
            new AssessmentSettings() {
                ExposureType = ExposureType.Chronic
            });
            var calculator = new ScreeningCalculatorFactory(settings, isPerPerson: false);
            calculator.Create();
            Assert.IsNotNull(calculator);
        }
    }
}
