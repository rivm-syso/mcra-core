using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class SamplesByFoodSummaryRecord {

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [DisplayName("Number of samples")]
        [Description("The number of the samples for this food product.")]
        public int NumberOfSamples { get; set; }

    }
}
