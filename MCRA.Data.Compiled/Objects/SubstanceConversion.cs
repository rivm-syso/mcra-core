namespace MCRA.Data.Compiled.Objects {
    public sealed class SubstanceConversion {
        public Compound MeasuredSubstance { get; set; }
        public Compound ActiveSubstance { get; set; }

        public double ConversionFactor { get; set; }
        public bool IsExclusive { get; set; }
        public double? Proportion { get; set; }
    }
}
