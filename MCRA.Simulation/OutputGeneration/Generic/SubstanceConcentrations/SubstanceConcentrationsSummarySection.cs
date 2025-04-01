namespace MCRA.Simulation.OutputGeneration {
    public class SubstanceConcentrationsSummarySection : SummarySection {
        public string ConcentrationUnit { get; set; }
        public List<SubstanceConcentrationsSummaryRecord> Records { get; set; }
        public List<SubstanceConcentrationsPercentilesRecord> PercentileRecords { get; set; }
    }
}
