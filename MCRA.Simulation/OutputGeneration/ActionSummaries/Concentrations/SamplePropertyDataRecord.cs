using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class SamplePropertyDataRecord {

        [DisplayName("Food name")]
        [Description("The name of the food.")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        [Description("The food code.")]
        public string FoodCode { get; set; }

        [DisplayName("Value")]
        [Description("The property value / category.")]
        public string PropertyValue { get; set; }

        [DisplayName("Number of samples")]
        [Description("The number of the selected food samples with this property value / category.")]
        public int NumberOfSamples { get; set; }

        [DisplayName("Percentage")]
        [Description("The percentage of the total selected food samples with this property value / category.")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentage { get; set; }

    }
}
