using MCRA.General;

namespace MCRA.Simulation.Calculators.HumanMonitoringCalculation.MissingValueImputationCalculators {
    public class HbmMissingValueImputationCalculatorFactory {

        public static IHbmMissingValueImputationCalculator Create(
            MissingValueImputationMethod missingValueImputationMethod
        ) {
            switch (missingValueImputationMethod) {
                case MissingValueImputationMethod.SetZero:
                    return new HbmMissingValueZeroImputationCalculator();
                case MissingValueImputationMethod.ImputeFromData:
                    return new HbmMissingValueRandomImputationCalculator();
                case MissingValueImputationMethod.NoImputation:
                    return new HbmMissingValueNoImputationCalculator();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
