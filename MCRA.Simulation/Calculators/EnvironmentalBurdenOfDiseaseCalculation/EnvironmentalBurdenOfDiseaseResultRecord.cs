using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public sealed class EnvironmentalBurdenOfDiseaseResultRecord {
        public BaselineBodIndicator BaselineBodIndicator { get; set; }
        public int ExposureBinId { get; set; }
        public ExposureInterval ExposureBin { get; set; }
        public PercentileInterval ExposurePercentileBin { get; set; }
        public double Exposure => ExposureResponseResultRecord.ExposureLevel;
        public DoseUnit ErfDoseUnit { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public double ResponseValue { get; set; }
        public double AttributableFraction { get; set; }
        public double TotalBod { get; set; }
        public double AttributableBod { get; set; }
        public ExposureResponseResultRecord ExposureResponseResultRecord { get; set; }
        public ExposureResponseFunction ExposureResponseFunction => ExposureResponseResultRecord.ExposureResponseFunction;
        public double CumulativeAttributableBod { get; set; }
    }
}
