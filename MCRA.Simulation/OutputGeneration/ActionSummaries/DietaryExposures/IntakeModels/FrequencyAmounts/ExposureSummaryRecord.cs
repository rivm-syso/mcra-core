using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ExposureSummaryRecord {
        [Description("Number of individuals in survey.")]
        [DisplayName("")]
        public string Description { get; set; }

        [Description("Number of observations.")]
        [DisplayName("Number of observations (N)")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberofObservations { get; set; }

        [Description("Mean.")]
        [DisplayName("Mean (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:F4}")]
        public double Mean { get; set; }

        [Description("Median.")]
        [DisplayName("Median (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:F4}")]
        public double Median { get; set; }

        [Description("Minimum.")]
        [DisplayName("Minimum (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:F4}")]
        public double Minimum { get; set; }

        [Description("Maximum.")]
        [DisplayName("Maximum (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:F4}")]
        public double Maximum { get; set; }

        [Description("Lower quartile.")]
        [DisplayName("Lower quartile (25%) (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:F4}")]
        public double LowerQuartile { get; set; }

        [Description("Upper quartile.")]
        [DisplayName("Upper quartile (75%) (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:F4}")]
        public double UpperQuartile { get; set; }
    }
}
