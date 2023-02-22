using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.SingleValueDietaryExposuresCalculation {
    public sealed class AcuteSingleValueDietaryExposureResult : ISingleValueDietaryExposure {
        public Food Food { get; set; }
        public Compound Substance { get; set; }
        public ProcessingType ProcessingType { get; set; }
        public SingleValueDietaryExposuresCalculationMethod CalculationMethod { get; set; }
        public IESTIType IESTICase { get; set; }
        public double LargePortion { get; set; }
        public double ConcentrationValue { get; set; }
        public double ProcessingFactor { get; set; }
        public bool MissingProcessingFactor { get; set; }
        public double UnitVariabilityFactor { get; set; }
        public QualifiedValue UnitWeightRac { get; set; }
        public QualifiedValue UnitWeightEp { get; set; }
        public double Exposure { get; set; }
        public ConcentrationValueType ConcentrationValueType { get; set; }
        public double OccurrenceFraction { get; set; }
        public double BodyWeight { get; set; }
        //public double ConversionFactor { get; set; }
    }
}
