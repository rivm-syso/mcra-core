namespace MCRA.Simulation.OutputGeneration {
    public sealed class KineticModelsSummarySection : SummarySection {
        public List<KineticModelSummaryRecord> Records { get; set; }
        public List<KineticModelSubstanceRecord> SubstanceGroupRecords { get; set; }
        public List<AbsorptionFactorRecord> AbsorptionFactorRecords { get; set; }
        public List<ParameterRecord> ParameterSubstanceIndependentRecords { get; set; }
        public List<ParameterRecord> ParameterSubstanceDependentRecords { get; set; }


    }
}
