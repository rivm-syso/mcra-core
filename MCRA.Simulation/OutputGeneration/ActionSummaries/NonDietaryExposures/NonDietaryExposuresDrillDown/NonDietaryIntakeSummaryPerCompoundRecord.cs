namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryIntakeSummaryPerCompoundRecord {
        public string SubstanceCode { get; set; }
        public string SubstanceName { get; set; }
        public double RelativePotencyFactor { get; set; }
        public List<RouteIntakeRecord> UncorrectedRouteIntakeRecords { get; set; }
        public double NonDietaryIntakeAmountPerBodyWeight { get; set; }
        public int NumberOfNondietaryContributions { get; set; }
    }
}
