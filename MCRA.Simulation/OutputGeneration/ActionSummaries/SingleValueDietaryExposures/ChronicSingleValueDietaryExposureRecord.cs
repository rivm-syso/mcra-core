using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class ChronicSingleValueDietaryExposureRecord {

        [Display(Name="Food name", Order=1)]
        public string FoodName { get; set; }

        [Display(Name = "Food code", Order = 2)]
        public string FoodCode { get; set; }

        [Display(Name = "Processing type", Order = 3)]
        public string ProcessingTypeName { get; set; }

        [Display(Name = "Processing type code", Order = 4)]
        public string ProcessingTypeCode { get; set; }

        [Display(Name = "Substance name", Order = 5)]
        public string SubstanceName { get; set; }

        [Display(Name = "Substance code", Order = 6)]
        public string SubstanceCode { get; set; }

        [Description("Mean consumption (ConsumptionUnit).")]
        [Display(Name = "Mean consumption (ConsumptionUnit)", Order = 10)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanConsumption { get; set; }

        [Description("Concentration (ConcentrationUnit).")]
        [Display(Name = "Concentration (ConcentrationUnit)", Order = 20)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConcentrationValue { get; set; }

        [Description("Concentration type.")]
        [Display(Name = "Concentration type", Order = 21)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public string ConcentrationValueType { get; set; }

        [Display(Name = "Processing factor", Order = 22)]
        [Description("The processing factor applied to the residue concentration")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? ProcessingFactor { get; set; }

        [Description("Exposure estimate for the population.")]
        [Display(Name = "Exposure (ExposureUnit)", Order = 30)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Exposure { get; set; }

        [Description("Use frequency.")]
        [Display(Name = "Use frequency", Order = 35)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double OccurrenceFraction { get; set; }

        [Description("Body weight (BodyWeightUnit).")]
        [Display(Name = "Body weight (BodyWeightUnit)", Order = 36)]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double BodyWeight { get; set; }

        [Display(Name = "Method", Order = 40)]
        public string CalculationMethod { get; set; }
    }
}

