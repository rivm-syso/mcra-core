namespace MCRA.Data.Compiled.Objects {
    public sealed class NonDietaryAbsorptionFactorxx {
        //public NonDietarySurvey NonDietarySurvey { get; set; }
        public Compound Compound { get; set; }
        public double DermalAbsorptionFactor { get; set; }
        public double OralAbsorptionFactor { get; set; }
        public double InhalationAbsorptionFactor { get; set; }
        public string NonDietarySurveyCode { get; set; }
    }
}
