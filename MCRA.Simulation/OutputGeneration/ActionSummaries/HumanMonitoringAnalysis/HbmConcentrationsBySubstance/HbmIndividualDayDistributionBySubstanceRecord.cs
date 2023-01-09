using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class HbmIndividualDayDistributionBySubstanceRecord {

        [Description("Target biological matrix.")]
        [DisplayName("Biological matrix")]
        public string BiologicalMatrix { get; set; }

        [Description("Source biological matrix (within parenthesis the number of samples).")]
        [DisplayName("Biological matrix: source")]
        public string SourceBiologicalMatrix { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Number of individual days with concentrations > 0.")]
        [DisplayName("Individual days with positive concentrations")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfDays { get; set; }

        [Description("Percentage of individual days with concentrations value > 0.")]
        [DisplayName("Percentage individual days with positive concentrations")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage { get; set; }

        [Description("Mean measurement value of all individual days (corrected for specific gravity correction factor).")]
        [DisplayName("Mean all individual days ({MonitoringConcentrationUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Total { get { return Mean * Percentage / 100; } }

        [Description("Median of measurement values of all individual days (corrected for specific gravity correction factor).")]
        [DisplayName("Median all individual days ({MonitoringConcentrationUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAll { get; set; }

        [Description("Lower percentile point of measurement values of all individual days (corrected for specific gravity correction factor).")]
        [DisplayName("{LowerPercentage} all individual days ({MonitoringConcentrationUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentileAll { get; set; }

        [Description("Upper percentile point of measurement values of all individual days (corrected for specific gravity correction factor).")]
        [DisplayName("{UpperPercentage} all individual days ({MonitoringConcentrationUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentileAll { get; set; }

        [Description("Average of measurement values of the individual days with measurement values > 0 (corrected for specific gravity correction factor).")]
        [DisplayName("Mean individual days positive concentrations ({MonitoringConcentrationUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mean { get; set; }

        [Description("Median of measurement values of the individual days with measurement values > 0 (corrected for specific gravity correction factor).")]
        [DisplayName("Median individual days positive concentrations ({MonitoringConcentrationUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median { get; set; }

        [Description("Lower percentile point of measurement values of the individual days with measurement values > 0 (corrected for specific gravity correction factor).")]
        [DisplayName("{LowerPercentage} individual days positive concentrations ({MonitoringConcentrationUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentilePositives { get; set; }

        [Description("Upper percentile point of measurement values of the individual days with measurement values > 0 (corrected for specific gravity correction factor).")]
        [DisplayName("{UpperPercentage} individual days positive concentrations ({MonitoringConcentrationUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentilePositives { get; set; }

    }
}
