using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OverallIndividualDrillDownRecord {

        [Display(AutoGenerateField = false)]
        public int SimulatedIndividualId { get; set; }

        [Description("Individual ID")]
        [DisplayName("Individual ID")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public string IndividualId { get; set; }

        [Description("Body weight.")]
        [DisplayName("Body weight (BodyWeightUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double BodyWeight { get; set; }

        [DisplayName("Cofactor")]
        public string Cofactor { get; set; }

        [DisplayName("Covariable")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Covariable { get; set; }

        [Description("Sampling weight.")]
        [DisplayName("Sampling  weight")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double SamplingWeight { get; set; }

        [Description("Sum of total exposure.")]
        [DisplayName("Total exposure (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double TotalIntake { get {return NumberOfSurveyDays * ObservedIndividualMean; } }

        [Description("Number of survey days.")]
        [DisplayName("Number of survey days")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfSurveyDays { get; set; }

        [Description("Number of positive survey days.")]
        [DisplayName("Number of positive survey days")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int PositiveSurveyDays { get; set; }

        [Description("Model based predicted frequency based on BetaBinomial or LogisticNormal model. The predicted frequencies are shrunken. Shrinkage factor is based on the realisation (positive survey days and total number of survey days). The predicted value is on the original scale (0, 1).")]
        [DisplayName("Frequency")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double FrequencyPrediction { get; set; }

        [Description("Predicted amount based on the Amounts model. The predicted value is on the transformed scale (or Modified BLUP on the transformed scale = (lp - (mean transformed exposure per day - lp) * sqrt(factor) and factor = VarianceBetween/(VarianceBetween + VarianceWithin/nDays) .")]
        [DisplayName("Group mean amount")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AmountPrediction { get; set; }

        [Description("Average of the power or logarithmic transformed daily exposures (positive days only).")]
        [DisplayName("Mean transformed exposure per day")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanTransformedIntake { get; set; }

        [Description("sqrt(VarianceBetween/(VarianceBetween + VarianceWithin/nDays)) with nDays = the number of positive survey days.")]
        [DisplayName("Amount shrinkage factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ShrinkageFactor { get; set; }

        [Description("Is based on the model estimated for the frequency and amounts model(= fitted frequency * BiasCorrectedBackTransformed (group mean amount + shrinkage factor * (mean transformed exposures per day - group mean amount))). For individuals without observed exposure, the model assisted exposure is simulated from the model(model-based imputation: ModelAssistedAmount = sqrt(VarianceBetween) * u ~StandardNormal() + Prediction")]
        [DisplayName("Model assisted exposure (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ModelAssistedExposure { get; set; }

        [Description("Observed Individual Mean, average of the daily exposures.")]
        [DisplayName("OIM (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ObservedIndividualMean { get; set; }
    }
}
