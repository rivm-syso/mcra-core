using MCRA.General;

namespace MCRA.Simulation.Calculators.SingleValueInternalExposuresCalculation {
    public interface ISingleValueInternalExposureCalculator {
        ICollection<ISingleValueNonDietaryExposure> Compute(
            TargetUnit targetUnit
        );
    }
}
