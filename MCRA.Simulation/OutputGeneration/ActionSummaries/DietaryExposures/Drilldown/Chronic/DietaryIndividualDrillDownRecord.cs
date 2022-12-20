using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryIndividualDrillDownRecord {
        [DisplayName("Individual ID")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public string IndividualId { get; set; }

        [DisplayName("Body weight")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BodyWeight { get; set; }

        [DisplayName("Sampling  weight")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double SamplingWeight { get; set; }

        [DisplayName("Covariable")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Covariable { get; set; }

        [DisplayName("Cofactor")]
        public string Cofactor { get; set; }

        [DisplayName("Number of survey days")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfSurveyDays { get; set; }

        [DisplayName("Number of positive survey days")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PositiveSurveyDays { get; set; }

        [DisplayName("Total exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double TotalIntake { get {return NumberOfSurveyDays * ObservedIndividualMean; } }

        [DisplayName("Fitted frequency")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double FrequencyPrediction { get; set; }

        [DisplayName("Model assisted frequency")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ModelAssistedFrequency { get; set; }

        [DisplayName("Group mean amount")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AmountPrediction { get; set; }

        [DisplayName("Mean transformed intakes per day")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanTransformedIntake { get; set; }

        [DisplayName("Shrinkage factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ShrinkageFactor { get; set; }

        [DisplayName("Model assisted amount")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ModelAssistedAmount { get; set; }

        [DisplayName("Model assisted intake")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ModelAssistedIntake { get; set; }

        [DisplayName("Observed Individual Mean")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ObservedIndividualMean { get; set; }

        [DisplayName("Others dietary intake")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double OthersDietaryIntakePerBodyWeight { get; set; }
    }
}
