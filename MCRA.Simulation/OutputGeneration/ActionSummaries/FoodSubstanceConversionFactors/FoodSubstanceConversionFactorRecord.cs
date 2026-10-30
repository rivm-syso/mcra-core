using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class FoodSubstanceConversionFactorRecord {

        [Description("Measured substance")]
        [DisplayName("Measured substance name")]
        public string MeasuredSubstanceName { get; set; }

        [Description("Measured substance")]
        [DisplayName("Measured substance code")]
        public string MeasuredSubstanceCode { get; set; }

        [Description("Active substance")]
        [DisplayName("Active substance name")]
        public string ActiveSubstanceName { get; set; }

        [Description("Active substance")]
        [DisplayName("Active substance code")]
        public string ActiveSubstanceCode { get; set; }

        [Description("Food")]
        [DisplayName("Foodname")]
        public string FoodName { get; set; }

        [Description("Food")]
        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Description("Food specific conversion factor")]
        [DisplayName("ConversionFactor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConversionFactor { get; set; }

        [Description("Reference")]
        [DisplayName("Reference")]
        public string Reference { get; set; }
    }
}
