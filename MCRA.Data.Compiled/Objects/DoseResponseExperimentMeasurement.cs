namespace MCRA.Data.Compiled.Objects {
    public sealed class DoseResponseExperimentMeasurement {
        public string IdExperiment { get; set; }
        public string IdExperimentalUnit { get; set; }
        public double Time { get; set; }
        public string idResponse { get; set; }
        public double ResponseValue { get; set; }
        public double? ResponseSD { get; set; }
        public double? ResponseCV { get; set; }
        public double? ResponseN { get; set; }
        public double? ResponseUncertaintyUpper { get; set; }
    }
}
