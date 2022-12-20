using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation {
    public class ChronicSingleValueDietaryExposureResult : ISingleValueDietaryExposure {
        public Food Food { get; set; }
        public ProcessingType ProcessingType { get; set; }
        public Compound Substance { get; set; }
        public SingleValueDietaryExposuresCalculationMethod CalculationMethod { get; set; }
        public double MeanConsumption { get; set; }
        public double ConcentrationValue { get; set; }
        public ConcentrationValueType ConcentrationValueType { get; set; }
        public double OccurrenceFraction { get; set; }
        public double ProcessingFactor { get; set; }
        public double Exposure { get; set; }
        public double BodyWeight { get; set; }
    }
}
