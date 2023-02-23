namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryChronicDrillDownRecord {
        public string Guid { get; set; }
        public string IndividualCode { get; set; }
        public double BodyWeight { get; set; }
        public double SamplingWeight { get; set; }
        public double ModelAssistedIntake { get; set; }
        public double ObservedIndividualMean { get; set; }
        public double FrequencyPrediction { get; set; }
        public double ModelAssistedFrequency { get; set; }
        public double AmountPrediction { get; set; }
        public double ShrinkageFactor { get; set; }
        public int PositiveSurveyDays { get; set; }
        public string Cofactor { get; set; }
        public string Covariable { get; set; }
        public double TransformedOIM { get; set; }
        public double DietaryIntakePerMassUnit { get; set; }
        public double OthersDietaryIntakePerMassUnit { get; set; }
        public double DietaryAbsorptionFactor { get; set; }
        public List<DietaryDayDrillDownRecord> DayDrillDownRecords { get; set; }
    }
}
