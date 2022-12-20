using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class SingleValueConsumptionSummaryRecord {

        [Display(Name = "Food name", Order = 1)]
        public string FoodName { get; set; }

        [Display(Name = "Food code", Order = 2)]
        public string FoodCode { get; set; }

        [Display(Name = "Processing type name", Order = 3)]
        public string ProcessingTypeName { get; set; }

        [Display(Name = "Processing type code", Order = 4)]
        public string ProcessingTypeCode { get; set; }

        [Description("Weight correction factor / reverse yield factor.")]
        [Display(Name = "Reverse yield factor", Order = 5)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ProportionProcessing { get; set; }

        [Description("Mean consumption amount (ConsumptionIntakeUnit).")]
        [Display(Name = "Mean consumption (ConsumptionIntakeUnit)", Order = 6)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanConsumption { get; set; }

        [Description("Median consumption amount (ConsumptionIntakeUnit).")]
        [Display(Name = "Median consumption (ConsumptionIntakeUnit)", Order = 7)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianConsumption { get; set; }

        [Description("Large portion (ConsumptionIntakeUnit).")]
        [Display(Name = "Large portion (ConsumptionIntakeUnit)", Order = 8)]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LargePortion { get; set; }

        [Description("Body weight (BodyWeightUnit).")]
        [Display(Name = "Body weight (BodyWeightUnit)", Order = 9)]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Bodyweight { get; set; }
    }
}
