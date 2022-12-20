using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class FoodExtrapolationsSummaryRecord {

        [DisplayName("Food name")]
        [Description("The name of the food for which missing data should be extrapolated.")]
        public string FoodFromName { get; set; }

        [DisplayName("Food code")]
        [Description("The code of the food for which missing data should be extrapolated.")]
        public string FoodFromCode { get; set; }

        [DisplayName("Read-across food name")]
        [Description("The name of the food from which data can be used.")]
        public string FoodToName { get; set; }

        [DisplayName("Read-across food code")]
        [Description("The code of the food from which data can be used.")]
        public string FoodToCode { get; set; }
    }
}
