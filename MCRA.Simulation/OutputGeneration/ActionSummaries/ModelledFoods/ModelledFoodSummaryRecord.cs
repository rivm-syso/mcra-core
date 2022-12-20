using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelledFoodSummaryRecord {

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [DisplayName("Has measurements")]
        public bool HasMeasurements { get; set; }

        [DisplayName("Has positive concentrations")]
        public bool HasPositiveConcentrations{ get; set; }

        [DisplayName("Has MRL")]
        public bool HasMrl{ get; set; }


    }
}
