namespace MCRA.Data.Compiled.Objects {
    public sealed class DeterministicSubstanceConversionFactor {
        public Compound MeasuredSubstance { get; set; }
        public Compound ActiveSubstance { get; set; }
        public Food Food { get; set; }
        public double ConversionFactor { get; set; }
        public string Reference { get; set; }
    }
}
