namespace MCRA.Simulation.OutputGeneration {
    /// <summary>
    /// Dietary and nondietary exposure per substance
    /// </summary>
    public sealed class DietaryIntakeSummaryPerCompoundRecord {
        public string CompoundCode { get; set; }
        public string CompoundName { get; set; }
        public double DietaryIntakeAmountPerBodyWeight { get; set; }
        public double RelativePotencyFactor { get; set; }
    }
}
