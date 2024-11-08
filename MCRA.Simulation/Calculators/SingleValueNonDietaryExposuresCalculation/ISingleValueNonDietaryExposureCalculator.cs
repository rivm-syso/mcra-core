using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.SingleValueNonDietaryExposuresCalculation {
    public interface ISingleValueNonDietaryExposureCalculator {
        ISingleValueNonDietaryExposure Compute(
            ICollection<Compound> substances,
            string codeConfig
        );
    }
}
