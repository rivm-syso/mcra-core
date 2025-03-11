using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ExposureBySourceRecord {

        [Description("Exposure source.")]
        [DisplayName("Exposure source")]
        public string ExposureSource { get; set; }

        [Description("Number of days for acute or number of individuals for chronic with exposure > 0.")]
        [DisplayName("{IndividualDayUnit} with exposure")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfDays { get; set; }

        [Description("Percentage of individual days (acute) or individuals (chronic) with exposure.")]
        [DisplayName("Percentage {IndividualDayUnit} with exposure > 0")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage { get; set; }

        [Description("Mean exposure for a source on all individual days (acute) or individuals (chronic).")]
        [DisplayName("Mean exposure for all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanAll { get; set; }

        [Description("p50 percentile all exposures per source.")]
        [DisplayName("Median for all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAll { get; set; }

        [Description("Percentile point for all exposures per source (default 25%, see Output settings).")]
        [DisplayName("{LowerPercentage} all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25All { get; set; }

        [Description("Percentile point for all exposures per source (default 75%, see Output settings).")]
        [DisplayName("{UpperPercentage} all {IndividualDayUnit} (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75All { get; set; }

        [Description("Average exposure value, for exposures > 0 per source.")]
        [DisplayName("Mean exposure {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mean { get; set; }

        [Description("p50 percentile for exposures > 0 per source.")]
        [DisplayName("Median {IndividualDayUnit} exposure > 0  (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Description("Percentile point for exposures > 0 per source (default 25%, see Output settings).")]
        [DisplayName("{LowerPercentage} {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile25 { get; set; }

        [Description("Percentile point for exposures > 0 per source (default 75%, see Output settings).")]
        [DisplayName("{UpperPercentage} {IndividualDayUnit} exposure > 0 (IntakeUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentile75 { get; set; }
    }
}
