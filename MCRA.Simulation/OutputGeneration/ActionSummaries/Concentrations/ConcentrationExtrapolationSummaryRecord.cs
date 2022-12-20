using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ConcentrationExtrapolationSummaryRecord {

        [Description("The food name.")]
        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [Description("The food code.")]
        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Description("The substance name.")]
        [DisplayName("Substance name")]
        public string ActiveSubstanceName { get; set; }

        [Description("The substance code.")]
        [DisplayName("Substance code")]
        public string ActiveSubstanceCode { get; set; }

        [Description("The number of measurements available for the food/substance combination.")]
        [DisplayName("Measurements (n)")]
        public int NumberOfMeasurements { get; set; }

        [Description("The name of the food from which concentrations may be extrapolated.")]
        [DisplayName("Extrapolated from name")]
        public string ExtrapolatedFoodName { get; set; }

        [Description("The code of the food from which concentrations may be extrapolated.")]
        [DisplayName("Extrapolated from food code")]
        public string ExtrapolatedFoodCode { get; set; }

        [Description("The name of the measured substance from which the concentrations may be extrapolated.")]
        [DisplayName("Measured substance name")]
        public string MeasuredSubstanceName { get; set; }

        [Description("The code of the measured substance from which the concentrations may be extrapolated.")]
        [DisplayName("Measured substance code")]
        public string MeasuredSubstanceCode { get; set; }

    }
}
