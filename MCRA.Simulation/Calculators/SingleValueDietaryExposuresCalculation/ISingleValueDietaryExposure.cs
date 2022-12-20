using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation {
    public interface ISingleValueDietaryExposure {
        Food Food { get; set; }
        Compound Substance { get; set; }
        ProcessingType ProcessingType { get; set; }
        SingleValueDietaryExposuresCalculationMethod CalculationMethod { get; set; }
        double Exposure { get; set; }
    }
}
