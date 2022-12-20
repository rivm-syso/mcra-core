using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class SingleValueConsumptionsDataSummaryRecord {

        [Description("Population name.")]
        [DisplayName("Population name")]
        public string PopulationName { get; set; }

        [Description("Population identifier.")]
        [DisplayName("Population id")]
        public string PopulationId { get; set; }

        [Description("Food name.")]
        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [Description("Food code.")]
        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Description("Consumption type (MC or percentile).")]
        [DisplayName("Consumption type")]
        public string ConsumptionType { get; set; }

        [Description("The specific percentile if the consumption type is specified as percentile.")]
        [DisplayName("Percentile")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile { get; set; }

        [Description("Deterministic consumption amount.")]
        [DisplayName("Consumption amount")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Amount { get; set; }

        [Description("Unit of the consumption amount.")]
        [DisplayName("Consumption unit")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public string Unit { get; set; }

        [Description("Reference to the source from which this value is obtained.")]
        [DisplayName("Reference")]
        public string Reference { get; set; }
    }
}
