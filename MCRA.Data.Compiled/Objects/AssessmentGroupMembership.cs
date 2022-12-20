namespace MCRA.Data.Compiled.Objects {
    public sealed class AssessmentGroupMembership {
        public string IdGroupMembershipModel  { get; set; }
        public Compound Compound { get; set; }
        public double MembershipProbability { get; set; }
    }
}
