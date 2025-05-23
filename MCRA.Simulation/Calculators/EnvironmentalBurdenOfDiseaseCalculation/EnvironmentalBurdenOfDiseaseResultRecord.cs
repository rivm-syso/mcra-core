using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Calculators.EnvironmentalBurdenOfDiseaseCalculation {
    public sealed class EnvironmentalBurdenOfDiseaseResultRecord {
        public BurdenOfDisease BurdenOfDisease { get; set; }
        public ExposureResponseFunction ExposureResponseFunction { get; set; }
        public DoseUnit ErfDoseUnit { get; set; }
        public TargetUnit TargetUnit { get; set; }
        public List<EnvironmentalBurdenOfDiseaseResultBinRecord> EnvironmentalBurdenOfDiseaseResultBinRecords { get; set; }
    }
}
