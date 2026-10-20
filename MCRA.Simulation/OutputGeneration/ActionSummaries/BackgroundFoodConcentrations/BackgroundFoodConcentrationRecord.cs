using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class BackgroundFoodConcentrationRecord {

        [Description("Name of the raw food.")]
        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [Description("Code of the raw food.")]
        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Description("Name of the substance for which this processing factor applies.")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Code of the substance for which this processing factor applies.")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("The (naturally occurring) background concentration level of the substance in the food.")]
        [DisplayName("Concentration")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Value { get; set; }

        [Description("The unit of measurement for the concentration.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Reference/description of the origin of the background concentration level estimate.")]
        public string Reference { get; set; }
    }
}
