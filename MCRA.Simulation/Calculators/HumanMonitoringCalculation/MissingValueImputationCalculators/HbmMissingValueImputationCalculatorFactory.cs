using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators {
    public class HbmMissingValueImputationCalculatorFactory {

        public static IHbmMissingValueImputationCalculator Create(
            MissingValueImputationMethod missingValueImputationMethod
        ) {
            if (missingValueImputationMethod == MissingValueImputationMethod.ImputeFromData) {
                return new HbmMissingValueRandomImputationCalculator();
            } else {
                return new HbmMissingValueZeroImputationCalculator();
            }
        }
    }
}
