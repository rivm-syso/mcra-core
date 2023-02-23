using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Simulation.Calculators.KineticModelCalculation;

namespace MCRA.Simulation.Calculators.TargetExposuresCalculation.TargetExposuresCalculators {
    public class TargetExposuresCalculatorFactory {

        public static ITargetExposuresCalculator Create(
            TargetLevelType targetDoseLevelType,
            IDictionary<Compound, IKineticModelCalculator> kineticModelCalculators
        ) {
            if (targetDoseLevelType == TargetLevelType.Internal) {
                return new InternalTargetExposuresCalculator(kineticModelCalculators);
            } else {
                return new ExternalTargetExposuresCalculator();
            }
        }
    }
}
