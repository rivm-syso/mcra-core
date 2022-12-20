using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UnMatchedModelledFoodSummaryRecord {

        [DisplayName("Modelled food name")]
        public string FoodAsMeasuredName { get; set; }

        [DisplayName("Modelled food code")]
        public string FoodAsMeasuredCode { get; set; }

    }
}
