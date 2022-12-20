using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DayFrequencyRecord {
        [Description("Number of days in survey.")]
        [DisplayName("Days")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int Days { get; set; }

        [Description("Percentage of days in survey.")]
        [DisplayName("Percentage days (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentageDays { get; set; }

        [Description("Number of individuals in survey.")]
        [DisplayName("Number of individuals (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfIndividuals { get; set; }

        [Description("Percentage of individuals in survey.")]
        [DisplayName("Percentage of individuals (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentageNumberOfIndividuals { get; set; }
    }
}
