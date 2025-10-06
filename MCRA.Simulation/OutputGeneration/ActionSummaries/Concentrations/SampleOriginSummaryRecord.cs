using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class SampleOriginSummaryRecord {

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [DisplayName("Origin")]
        [Description("The origin of the samples.")]
        public string Origin { get; set; }

        [DisplayName("Number of samples")]
        [Description("The number of the samples from this origin.")]
        public int NumberOfSamples { get; set; }

        [DisplayName("Percentage")]
        [Description("The percentage of food samples with this origin.")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentage { get; set; }

    }
}
