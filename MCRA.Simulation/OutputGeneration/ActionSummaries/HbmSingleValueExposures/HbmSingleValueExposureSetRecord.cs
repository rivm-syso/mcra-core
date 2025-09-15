namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmSingleValueExposureSetRecord {
        public string SubstanceCode { get; set; }
        public string SubstanceName { get; set; }
        public string BiologicalMatrix { get; set; }
        public string DoseUnit { get; set; }
        public List<HbmSingleValueExposurePercentileRecord> PercentileRecords { get; set; }
    }
}
