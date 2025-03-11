using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public sealed class EnvironmentalBurdenOfDiseaseResultRecord {
        public BodIndicator BodIndicator { get; set; }
        public PercentileInterval ExposureBin { get; set; }
        public double Exposure => ExposureEffectResultRecord.ExposureLevel;
        public string Unit { get; set; }
        public double Ratio { get; set; }
        public double AttributableFraction { get; set; }
        public double TotalBod { get; set; }
        public double AttributableBod { get; set; }
        public ExposureEffectResultRecord ExposureEffectResultRecord { get; set; }
        public ExposureEffectFunction ExposureEffectFunction => ExposureEffectResultRecord.ExposureEffectFunction;
    }
}
