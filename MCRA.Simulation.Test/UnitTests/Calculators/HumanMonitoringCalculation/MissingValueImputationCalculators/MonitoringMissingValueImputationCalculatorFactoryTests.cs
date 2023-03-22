using MCRA.General;
using MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCRA.Simulation.Test.UnitTests.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators {
    [TestClass]
    public class MonitoringMissingValueImputationCalculatorFactoryTests {

        [TestMethod]
        [DataRow(MissingValueImputationMethod.ImputeFromData, typeof(HbmMissingValueRandomImputationCalculator))]
        [DataRow(MissingValueImputationMethod.SetZero, typeof(HbmMissingValueZeroImputationCalculator))]
        [DataRow(MissingValueImputationMethod.NoImputation, typeof(HbmMissingValueNoImputationCalculator))]
        public void MonitoringMissingValueImputationCalculatorFactory_TestCreate(
            MissingValueImputationMethod method,
            Type type
        ) {
            var result = HbmMissingValueImputationCalculatorFactory.Create(method);
            Assert.IsTrue(result.GetType() == type);
        }
    }
}
