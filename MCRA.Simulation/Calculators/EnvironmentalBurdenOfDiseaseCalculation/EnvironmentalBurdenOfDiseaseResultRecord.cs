using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public sealed class EnvironmentalBurdenOfDiseaseResultRecord {
        public PercentileInterval PercentileInterval { get; set; }
        public double ExposureLevel => ExposureEffectResultRecord.ExposureLevel;
        public string Unit { get; set; }
        public double PercentileSpecificOr { get; set; }
        public double PercentileSpecificAf { get; set; }
        public double AbsoluteBod { get; set; }
        public double AttributableEbd { get; set; }
        public ExposureEffectResultRecord ExposureEffectResultRecord { get; set; }
        public ExposureEffectFunction ExposureEffectFunction => ExposureEffectResultRecord.ExposureEffectFunction;
    }
}
