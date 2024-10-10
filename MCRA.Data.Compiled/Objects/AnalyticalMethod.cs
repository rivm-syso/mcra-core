namespace MCRA.Data.Compiled.Objects {
    public sealed class AnalyticalMethod: StrongEntity {
        public int SampleCount { get; set; }
        public IDictionary<Compound, AnalyticalMethodCompound> AnalyticalMethodCompounds { get; set; } = new Dictionary<Compound, AnalyticalMethodCompound>();
    }
}
