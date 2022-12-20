namespace MCRA.Data.Raw.Copying.BulkCopiers.DoseResponse {

    /// <summary>
    /// Dose Response Measurement record
    /// </summary>
    public class ExperimentalUnitPropertyRecord {
        public string IdExperiment { get; set; }
        public string IdExperimentalUnit { get; set; }
        public string PropertyName { get; set; }
        public string Value { get; set; }
    }
}
