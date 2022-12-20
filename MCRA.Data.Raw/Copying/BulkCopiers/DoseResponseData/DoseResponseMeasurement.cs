namespace MCRA.Data.Raw.Copying.BulkCopiers.DoseResponse {

    public enum ResponseValueType {
        Value,
        SD,
        CV,
        N,
        Uncertainty,
    }

    /// <summary>
    /// Dose Response Measurement record
    /// </summary>
    public sealed class DoseResponseMeasurementRecord {
        public string IdExperiment { get; set; }
        public string IdExperimentalUnit { get; set; }
        public double? Time { get; set; }
        public string IdResponse { get; set; }
        public double ResponseValue { get; set; }
        public double? ResponseSD { get; set; }
        public double? ResponseCV { get; set; }
        public double? ResponseN { get; set; }
        public double? ResponseUncertaintyUpper { get; set; }
    }
}
