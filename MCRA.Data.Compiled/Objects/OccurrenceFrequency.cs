namespace MCRA.Data.Compiled.Objects {
    public sealed class OccurrenceFrequency {
        public Compound Substance { get; set; }
        public Food Food { get; set; }
        public double Percentage { get; set; }
        public string Reference { get; set; }
    }
}
