namespace MCRA.Data.Compiled.Objects {
    public sealed class EffectRelationship {
        public AdverseOutcomePathwayNetwork AdverseOutcomePathwayNetwork { get; set; }
        public Effect UpstreamKeyEvent { get; set; }
        public Effect DownstreamKeyEvent { get; set; }
    }
}
