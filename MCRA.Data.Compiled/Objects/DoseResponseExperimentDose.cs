namespace MCRA.Data.Compiled.Objects {
    public sealed class DoseResponseExperimentDose {
        public string IdExperiment { get; set; }
        public string IdExperimentalUnit { get; set; }
        public double? Time { get; set; }
        public Compound IdSubstance { get; set; }
        public double Dose { get; set; }
    }
}
