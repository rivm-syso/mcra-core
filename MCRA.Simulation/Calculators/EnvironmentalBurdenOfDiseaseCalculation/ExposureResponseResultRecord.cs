using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public sealed class ExposureResponseResultRecord {
        public ExposureResponseFunction ExposureResponseFunction { get; set; }
        public bool IsErfDefinedExposureBin { get; set; }
        public int ExposureBinId { get; set; }
        public ExposureResponseType ExposureResponseType { get; set; }
        public PercentileInterval PercentileInterval { get; set; }
        public ExposureInterval ExposureInterval { get; set; }
        public double ErfDoseUnitAlignmentFactor { get; set; }
        public double ExposureLevel { get; set; }
        public double PercentileSpecificRisk { get; set; }
        public Compound Substance { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public EffectMetric EffectMetric { get; set; }
    }
}
