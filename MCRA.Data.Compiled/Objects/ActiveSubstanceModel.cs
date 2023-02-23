namespace MCRA.Data.Compiled.Objects {
    public sealed class ActiveSubstanceModel : IStrongEntity {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Effect Effect { get; set; }
        public Compound IndexSubstance { get; set; }
        public string Reference { get; set; }

        public double? Accuracy { get; set; }
        public double? Sensitivity { get; set; }
        public double? Specificity { get; set; }

        public IDictionary<Compound, double> MembershipProbabilities { get; set; } = new Dictionary<Compound, double>();

        public override string ToString() {
            return $"[{GetHashCode():X8}] {Code}";
        }
    }
}
