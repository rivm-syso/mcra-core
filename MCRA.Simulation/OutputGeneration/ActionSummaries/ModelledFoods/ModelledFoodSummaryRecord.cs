using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelledFoodSummaryRecord {

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

    }
}
