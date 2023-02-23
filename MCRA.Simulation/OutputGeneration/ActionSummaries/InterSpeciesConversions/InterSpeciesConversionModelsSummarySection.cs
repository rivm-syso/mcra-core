namespace MCRA.Simulation.OutputGeneration {
    public sealed class InterSpeciesConversionModelsSummarySection : SummarySection {
        public override bool SaveTemporaryData => true;

        public InterSpeciesConversionModelSummaryRecord DefaultInterSpeciesFactor { get; set; }
        public List<InterSpeciesConversionModelSummaryRecord> Records { get; set; }
    }
}
