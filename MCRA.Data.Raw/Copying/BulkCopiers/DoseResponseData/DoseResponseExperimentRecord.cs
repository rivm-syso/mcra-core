namespace MCRA.Data.Raw.Copying.BulkCopiers.DoseResponse {

    /// <summary>
    /// Dose Response Experiment record
    /// </summary>
    public sealed class DoseResponseExperimentRecord {
        public string idExperiment { get; set; }
        public string ExperimentalUnit { get; set; }
        public string Substances { get; set; }
        public string Responses { get; set; }
        public string Time { get; set; }
        public string Covariates { get; set; }
    }
}
