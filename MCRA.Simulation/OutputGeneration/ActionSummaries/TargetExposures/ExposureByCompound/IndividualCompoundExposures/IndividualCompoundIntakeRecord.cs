using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IndividualCompoundIntakeRecord {

        public IndividualCompoundIntakeRecord() { }

        [Description("Individual ID")]
        [DisplayName("IndividualID")]
        public string IndividualId { get; set; }

        [Description("Sampling Weight")]
        [DisplayName("SamplingWeight")]
        public double SamplingWeight { get; set; }

        [Description("Body weight")]
        [DisplayName("Bodyweight")]
        public double Bodyweight { get; set; }

        [Description("Total number of survey days")]
        [DisplayName("DaysInSurvey")]
        public int NumberOfDaysInSurvey { get; set; }

        [Description("Substance code")]
        [DisplayName("SubstanceCode")]
        public string SubstanceCode { get; set; }

        [Description("Exposure amount (TargetUnit)")]
        [DisplayName("Exposure amount (TargetUnit)")]
        public double Exposure { get; set; }

        [Description("Cumulative exposure amount (TargetAmountUnit) expressed in terms of reference compound equivalents and corrected by assessment group membership probability")]
        [DisplayName("Cumulative exposure amount (TargetAmountUnit)")]
        public double CumulativeExposure { get; set; }

    }
}
