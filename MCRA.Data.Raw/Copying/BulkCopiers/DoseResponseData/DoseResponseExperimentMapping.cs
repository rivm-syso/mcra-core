namespace MCRA.Data.Raw.Copying.BulkCopiers.DoseResponse {
    public sealed class DoseResponseExperimentMapping {
        public Dictionary<string, ResponseMapping> ResponseMappings { get; set; }
        public Dictionary<string, int> SubstanceMappings { get; set; }
        public Dictionary<string, int> CovariateMappings { get; set; }
        public int? TimeField { get; set; }
        public Dictionary<string, int> ExperimentalUnitMappings { get; set; }
    }
}
