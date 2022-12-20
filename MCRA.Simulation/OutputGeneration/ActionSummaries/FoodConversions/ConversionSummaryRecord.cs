using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConversionSummaryRecord {

        [Display(AutoGenerateField = false)]
        public int IdCompound { get; set; }


        [DisplayName("Food as eaten name")]
        [Description("Name of the consumed food.")]
        public string FoodAsEatenName { get; set; }

        [DisplayName("Food as eaten code")]
        [Description("Code of the consumed food.")]
        public string FoodAsEatenCode { get; set; }

        [DisplayName("Modelled food name")]
        [Description("Name of the modelled food.")]
        public string FoodAsMeasuredName { get; set; }

        [DisplayName("Modelled food code")]
        [Description("Code of the modelled food.")]
        public string FoodAsMeasuredCode { get; set; }

        [DisplayName("Proportion")]
        [Description("Proportion/fraction of the modelled food in the consumed food.")]
        public double Proportion { get; set; }

        [DisplayName("Market share")]
        [Description("For sub types: the market share of the sub type with respect to the super type.")]
        public double MarketShare { get; set; }

        [DisplayName("Processing type name")]
        [Description("For processing conversions: name of the processing type.")]
        public string ProcessingTypeName { get; set; }

        [DisplayName("Processing type code")]
        [Description("For processing conversions: code of the processing type.")]
        public string ProcessingTypeCode { get; set; }

        [DisplayName("Proportion processed modelled food")]
        [Description("For processing conversions: proportion of the processed food relative to the consumed food.")]
        public double ProportionProcessedFoodAsMeasured { get; set; }

        [DisplayName("Proportion processed")]
        [Description("For processing conversions: fraction of the proportion associated with processing.")]
        public double ProportionProcessed { get; set; }

        [Display(AutoGenerateField = true)]
        [DisplayName("Substance code")]
        [Description("For substance specific convertions: the code of substance for which the conversion was done.")]
        public string CompoundCode { get; set; }

        [Display(AutoGenerateField = true)]
        [DisplayName("Substance name")]
        [Description("For substance specific convertions: the name of substance for which the conversion was done.")]
        public string CompoundName { get; set; }

        [Display(AutoGenerateField = false)]
        [DisplayName("Number of conversion steps")]
        [Description("The number of conversion steps.")]
        public int Steps { get; set; }

        [DisplayName("Conversion steps")]
        [Display(AutoGenerateField = false)]
        public ConversionStepRecords ConversionStepResults { get; set; }

        [DisplayName("Conversion steps")]
        [Description("The steps of the conversion in terms of traversed food codes.")]
        public string ConversionSteps {
            get {
                return (Steps > 0) ? ConversionStepResults.ToString() : "No steps";
            }
        }
    }

    public sealed class ConversionStepRecords : List<ConversionStepRecord> {
        public override string ToString() {
            var stepStrings = this.Select(c => $"{c.Step.GetDisplayAttribute().ShortName}: {c.FoodCodeTo}").ToList();
            return this.First().FoodCodeFrom + " => " + string.Join("=>", stepStrings);
        }
    }
}
