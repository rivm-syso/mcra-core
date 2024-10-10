namespace MCRA.Data.Compiled.Objects {
    public sealed class QsarMembershipModel: StrongEntity {
        public Effect Effect { get; set; }
        public double? Accuracy { get; set; }
        public double? Sensitivity { get; set; }
        public double? Specificity { get; set; }
        public string Reference { get; set; }

        public IDictionary<Compound, double> MembershipScores { get; set; }
    }
}
