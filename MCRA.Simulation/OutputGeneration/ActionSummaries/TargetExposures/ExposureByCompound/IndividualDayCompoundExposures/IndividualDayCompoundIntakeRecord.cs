using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class IndividualDayCompoundIntakeRecord {

        public IndividualDayCompoundIntakeRecord() { }

        [Description("ID of the simulated individual day")]
        [DisplayName("SimulatedIndividualDayID")]
        public string SimulatedIndividualDayId { get; set; }

        [Description("ID of the dietary survey individual used for simulation")]
        public string DietarySurveyIndividualCode { get; set; }

        [Description("ID of the dietary survey day used for simulation")]
        public string DietarySurveyDayCode { get; set; }

        [Description("Sampling Weight")]
        public double SamplingWeight { get; set; }

        [Description("Body weight")]
        [DisplayName("Bodyweight")]
        public double Bodyweight { get; set; }

        [Description("Relative compartment weight")]
        [DisplayName("Relative compartment weight")]
        public double RelativeCompartmentWeight { get; set; }

        [Description("Substance code")]
        [DisplayName("SubstanceCode")]
        public string SubstanceCode { get; set; }

        [Description("Exposure amount (TargetAmountUnit)")]
        [DisplayName("Exposure amount (TargetAmountUnit)")]
        public double ExposureAmount { get; set; }

        [Description("Exposure concentration (TargetConcentrationUnit)")]
        [DisplayName("Exposure concentration (TargetConcentrationUnit)")]
        public double ExposureConcentration {
            get {
                return ExposureAmount / (Bodyweight * RelativeCompartmentWeight);
            }
        }

        [Description("Cumulative exposure amount (TargetAmountUnit) expressed in terms of reference compound equivalents and corrected by assessment group membership probability")]
        [DisplayName("Cumulative exposure amount (TargetAmountUnit)")]
        public double CumulativeExposure { get; set; }

        [Description("Cumulative exposure concentration (TargetConcentrationUnit)")]
        [DisplayName("Cumulative exposure concentration (TargetConcentrationUnit)")]
        public double CumulativeExposureConcentration {
            get {
                return CumulativeExposure / (Bodyweight * RelativeCompartmentWeight);
            }
        }
    }
}
