using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Helper class for modelled foods at risk.
    /// </summary>
    public sealed class ModelledFoodAtRiskRecord {

        [DisplayName("Food name")]
        [Description("Name of the modelled food.")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        [Description("Code of the modelled food.")]
        public string FoodCode { get; set; }

        [Description("Percentage of individual-days (acute) or individuals (chronic) where the risk threshold is exceeded specifically due to the food.")]
        [DisplayName("At risk due to food (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double AtRiskDueToFood { get; set; }

        [Description("Percentage of individual-days (acute) or individuals (chronic) where the risk threshold is not exceeded.")]
        [DisplayName("Not at risk (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double NotAtRisk { get; set; }

        [Description("Percentage of individual-days (acute) or individuals (chronic) where the risk threshold is exceeded already by contributions from other foods.")]
        [DisplayName("At risk with or without food (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double AtRiskWithOrWithout { get; set; }
    }
}
