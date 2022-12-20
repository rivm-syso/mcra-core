namespace MCRA.Data.Raw.Copying.BulkCopiers.DoseResponse {

    public enum DoseResponseDataField {
        idDataSource,
        Experiment,
        Substances,
        Responses,
        Time,
        Covariates,
    }

    /// <summary>
    /// Dose Response Experiment Dose record
    /// </summary>
    public sealed class DoseResponseDoseRecord {
        public string IdExperiment { get; set; }
        public string IdExperimentalUnit { get; set; }
        public double? Time { get; set; }
        public string IdSubstance { get; set; }
        public double Dose { get; set; }
    }
}
