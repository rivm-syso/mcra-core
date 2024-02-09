using MCRA.General;

namespace MCRA.Data.Compiled.Objects {
    public sealed class ExposureEstimate {
        public ExposureScenario ExposureScenario { get; set; }
        public ExposureDeterminantCombination ExposureDeterminantCombination { get; set; }
        public string ExposureSource { get; set; }
        public Compound Substance { get; set; }
        public ExposureRoute ExposureRoute { get; set; }
        public double Value { get; set; }
        public string EstimateType { get; set; }
    }
}
