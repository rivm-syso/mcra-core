using MCRA.General;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class AcuteSingleValueDietaryExposureRecord {

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [DisplayName("Processing type")]
        public string ProcessingTypeName { get; set; }

        [DisplayName("Processing type code")]
        public string ProcessingTypeCode { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Exposure estimate for the population")]
        [DisplayName("Exposure (ExposureUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Exposure { get; set; }

        [Description("Large portion consumption amount (ConsumptionUnit).")]
        [DisplayName("Large portion (ConsumptionUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LargePortion { get; set; }

        [DisplayName("Processing factor")]
        [Description("The processing factor applied to the residue concentration")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? ProcessingFactor { get; set; }

        [Display(AutoGenerateField = false)]
        public QualifiedValue UnitWeightEpQualifiedValue { get; set; }

        [Description("Unit weight edible portion (ConsumptionUnit).")]
        [DisplayName("Unit weight EP (ConsumptionUnit)")]
        public string UnitWeightEp {
            get {
                return UnitWeightEpQualifiedValue?.ToString("G3");
            }
        }

        [Display(AutoGenerateField = false)]
        public QualifiedValue UnitWeightRacQualifiedValue { get; set; }

        [Description("Unit weight RAC (ConsumptionUnit).")]
        [DisplayName("Unit weight RAC (ConsumptionUnit)")]
        public string UnitWeightRac {
            get {
                return UnitWeightRacQualifiedValue?.ToString("G3");
            }
        }

        [Description("Unit variability factor")]
        [DisplayName("Unit variability factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UnitVariabilityFactor { get; set; }

        [Description("Concentration (ConcentrationUnit).")]
        [Display(Name = "Concentration (ConcentrationUnit)", Order = 20)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConcentrationValue { get; set; }

        [Description("Concentration type.")]
        [Display(Name = "Concentration type", Order = 21)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public string ConcentrationValueType { get; set; }

        [Description("Use frequency.")]
        [Display(Name = "Use frequency", Order = 22)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double OccurrenceFraction { get; set; }

        [Description("Body weight (BodyWeightUnit).")]
        [Display(Name = "Body weight (BodyWeightUnit)", Order = 23)]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double BodyWeight { get; set; }

        [Display(Name="Method", Order = 24)]
        public string CalculationMethod { get; set; }
    }
}

