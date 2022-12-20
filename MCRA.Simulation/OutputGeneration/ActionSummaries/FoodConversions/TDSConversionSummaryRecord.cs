using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TDSConversionSummaryRecord {

        [Display(AutoGenerateField = false)]
        public int IdCompound { get; set; }

        [Display(AutoGenerateField = false)]
        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Display(AutoGenerateField = false)]
        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [Description("Food can be a food as eaten or composition or ingredient")]
        [DisplayName("Food name")]
        public string FoodAsEatenName { get; set; }

        [DisplayName("Food code")]
        public string FoodAsEatenCode { get; set; }

        [DisplayName("TDS food name")]
        public string FoodAsMeasuredName { get; set; }

        [DisplayName("TDS food code")]
        public string FoodAsMeasuredCode { get; set; }

        [DisplayName("Regionality")]
        public string Regionality { get; set; }

        [DisplayName("Seasonality")]
        public string Seasonality { get; set; }

        [DisplayName("Total weight or volume")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double NumberOfSamples { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }
    }
}
