using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ConcentrationLimitExceedanceDataRecord {

        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Limit value (ConcentrationUnit).")]
        [DisplayName("Limit value (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LimitValue { get; set; }

        [DisplayName("Total samples analysed")]
        [Description("The total number of analysed samples.")]
        public int TotalNumberOfAnalysedSamples { get; set; }

        [DisplayName("Samples exceeding threshold")]
        [Description("The number of the samples exceeding the concentration limit threshold.")]
        public int NumberOfSamplesExceedingLimit { get; set; }

        [DisplayName("Fraction of total")]
        [Description("The fraction of the total number of analysed samples exceeding the concentration limit threshold.")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double FractionOfTotal { get; set; }

    }
}
