using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators {
    [TestClass]
    public class MissingValueImputationCalculatorsTests {
        [TestMethod]
        public void MonitoringMissingValueImputationCalculatorFactory_TestCreate() {
            var result = HbmMissingValueImputationCalculatorFactory.Create(MissingValueImputationMethod.ImputeFromData);
            Assert.IsTrue(result is HbmMissingValueRandomImputationCalculator);
            result = HbmMissingValueImputationCalculatorFactory.Create(MissingValueImputationMethod.SetZero);
            Assert.IsTrue(result is HbmMissingValueZeroImputationCalculator);
        }
    }
}
