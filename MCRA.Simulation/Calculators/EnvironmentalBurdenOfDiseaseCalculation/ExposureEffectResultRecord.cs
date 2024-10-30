using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public sealed class ExposureEffectResultRecord {
        public ExposureEffectFunction ExposureEffectFunction;
        public PercentileInterval PercentileInterval { get; set; }
        public double ExposureLevel { get; set; }
        public double PercentileSpecificRisk { get; set; }
        public Compound Substance { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public EffectMetric EffectMetric { get; set; }
    }
}
