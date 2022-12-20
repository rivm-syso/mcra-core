namespace MCRA.Data.Compiled.Objects {
    public sealed class IntraSpeciesFactor {
        public Compound Compound { get; set; }
        public Effect Effect { get; set; }

        public string IdPopulation { get; set; }
        public double Factor { get; set; } = 1;
        public double? LowerVariationFactor { get; set; }
        public double UpperVariationFactor { get; set; }
    }
}
