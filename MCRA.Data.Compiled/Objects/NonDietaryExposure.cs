namespace MCRA.Data.Compiled.Objects {
    public sealed class NonDietaryExposure {
        public Compound Compound { get; set; }
        public double Dermal { get; set; }
        public double Oral { get; set; }
        public double Inhalation { get; set; }
        public string NonDietarySetCode { get; set; }
        public string IdIndividual { get; set; }
    }
}
