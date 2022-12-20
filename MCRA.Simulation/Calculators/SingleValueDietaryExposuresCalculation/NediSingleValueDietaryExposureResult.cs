using MCRA.General;

namespace MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation {
    public sealed class NediSingleValueDietaryExposureResult : ChronicSingleValueDietaryExposureResult {
        public double LargePortion { get; set; }
        public double HighExposure { get; set; }
    }
}
