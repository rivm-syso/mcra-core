namespace MCRA.Simulation.OutputGeneration {
    public class DrillDownRecordSection<TRecord> : SummarySection {
        public TRecord DrillDownRecord { get; set; }
        public string ReferenceSubstanceName { get; set; }
        public bool IsCumulative { get; set; }
        public bool UseProcessing { get; set; }
        public bool UseUnitVariability { get; set; }
        public int DisplayNumber = 10;
    }
}
