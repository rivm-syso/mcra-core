using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators {
    public class HbmMissingValueImputationCalculatorFactory {

        public static IHbmMissingValueImputationCalculator Create(
            MissingValueImputationMethod missingValueImputationMethod
        ) {
            if (missingValueImputationMethod == MissingValueImputationMethod.ImputeFromData) {
                return new HbmMissingValueRandomImputationCalculator();
            } else if (missingValueImputationMethod == MissingValueImputationMethod.SetZero) {
                return new HbmMissingValueZeroImputationCalculator();
            } else {
                return new HbmMissingValueNoImputationCalculator();
            }
        }
    }
}
