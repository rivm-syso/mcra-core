namespace MCRA.Data.Compiled.Objects {
    public sealed class RelativePotencyFactor {
        public RelativePotencyFactor() {
            RelativePotencyFactorsUncertains = new HashSet<RelativePotencyFactorUncertain>();
        }
        public Compound Compound { get; set; }
        public Effect Effect { get; set; }
        public double RPF { get; set; }
        public string PublicationTitle { get; set; }
        public string PublicationAuthors { get; set; }
        public int? PublicationYear { get; set; }
        public string PublicationUri { get; set; }
        public string Description { get; set; }
        public ICollection<RelativePotencyFactorUncertain> RelativePotencyFactorsUncertains { get; set; }
    }
}
