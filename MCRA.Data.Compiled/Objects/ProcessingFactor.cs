namespace MCRA.Data.Compiled.Objects {
    public sealed class ProcessingFactor {
        public double Nominal { get; set; }
        public double? Upper { get; set; }
        public double? NominalUncertaintyUpper { get; set; }
        public double? UpperUncertaintyUpper { get; set; }

        public Compound Compound { get; set; }
        public Food FoodProcessed { get; set; }
        public Food FoodUnprocessed { get; set; }
        public ProcessingType ProcessingType { get; set; }
    }
}
