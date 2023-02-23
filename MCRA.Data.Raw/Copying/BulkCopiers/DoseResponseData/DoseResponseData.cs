namespace MCRA.Data.Raw.Copying.BulkCopiers.DoseResponse {

    /// <summary>
    /// Dose Response Measurement record
    /// </summary>
    public sealed class DoseResponseData {
        public List<DoseResponseDoseRecord> DoseResponseDoseRecords { get; set; }
        public List<DoseResponseMeasurementRecord> DoseResponseMeasurementRecords { get; set; }
        public List<ExperimentalUnitPropertyRecord> ExperimentalUnitProperties { get; set; }
    }
}
