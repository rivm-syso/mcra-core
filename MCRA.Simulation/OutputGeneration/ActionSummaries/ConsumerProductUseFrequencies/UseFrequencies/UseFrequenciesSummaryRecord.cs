using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UseFrequenciesSummaryRecord {

        [DisplayName("Consumer product name")]
        public string Name { get; set; }

        [DisplayName("Consumer product code")]
        public string Code { get; set; }

        [Description("Total number of individuals.")]
        [DisplayName("Total number of individuals")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfIndividualDays { get; set; }

        [Description("Percentage of individuals using a consumer product.")]
        [DisplayName("Percentage of individuals with use")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentageOfIndividualDaysWithUse { get; set; }

        [Description("Mean of use frequencies.")]
        [DisplayName("Mean")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mean { get; set; }

        [Description("Median of use frequencies.")]
        [DisplayName("Median")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Description("Percentile point of use frequencies (default 25%, see Output settings).")]
        [DisplayName("{LowerPercentage} for all individuals")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25All { get; set; }

        [Description("Percentile point of use frequencies  (default 75%, see Output settings).")]
        [DisplayName("{UpperPercentage} for all individuals")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75All { get; set; }
    }
}
