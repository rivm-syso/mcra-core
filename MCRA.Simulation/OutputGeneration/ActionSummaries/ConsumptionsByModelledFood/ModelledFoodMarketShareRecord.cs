using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Holds market share information for a given food/compound combination.
    /// </summary>
    public sealed class ModelledFoodMarketShareRecord {
        [Description("Super type, the consumed food is on a higher hierarchical level than the food market share")]
        [DisplayName("Food name consumed")]
        public string ConsumedFoodName { get; set; }

        [Description("Super type")]
        [DisplayName("Food code consumed")]
        public string ConsumedFoodCode { get; set; }

        [Description("Market share name, concentration values are on the level of food market shares")]
        [DisplayName("Market share name")]
        public string FoodName { get; set; }

        [DisplayName("Market share code")]
        public string FoodCode { get; set; }

        [Display(AutoGenerateField = false)]
        public double MarketShare { get; set; }

        [Description("Market share percentage: shares do not necessarily sum to 100%")]
        [DisplayName("Market share (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double MarketSharePercentage { get { return MarketShare * 100; } }
    }
}
