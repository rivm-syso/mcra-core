namespace MCRA.Data.Compiled.Objects {
    public sealed class NonDietaryExposure {
        public string IdIndividual { get; set; }
        public Compound Compound { get; set; }
        public double Dermal { get; set; }
        public double Oral { get; set; }
        public double Inhalation { get; set; }
    }
}
