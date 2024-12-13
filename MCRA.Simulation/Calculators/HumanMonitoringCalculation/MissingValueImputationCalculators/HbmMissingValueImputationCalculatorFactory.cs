using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators {
    public class HbmMissingValueImputationCalculatorFactory {

        public static IHbmMissingValueImputationCalculator Create(
            MissingValueImputationMethod missingValueImputationMethod
        ) {
            return missingValueImputationMethod switch {
                MissingValueImputationMethod.SetZero => new HbmMissingValueZeroImputationCalculator(),
                MissingValueImputationMethod.ImputeFromData => new HbmMissingValueRandomImputationCalculator(),
                MissingValueImputationMethod.NoImputation => new HbmMissingValueNoImputationCalculator(),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
