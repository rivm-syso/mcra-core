using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ConcentrationLimitExceedanceByFoodDataRecord {

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [DisplayName("Total samples analysed")]
        [Description("The total number of food samples.")]
        public int TotalNumberOfSamples { get; set; }

        [DisplayName("Samples exceeding threshold")]
        [Description("The number of food samples for which any of the substance concentrations exceeds the concentration limit threshold.")]
        public int NumberOfSamplesExceedingLimit { get; set; }

        [DisplayName("Fraction of total")]
        [Description("The fraction of the total number of food samples for which any of the substance concentrations exceeds the concentration limit threshold.")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double FractionOfTotal { get; set; }

    }
}
