using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData {
    public sealed class HbmSampleConcentrationPercentilesRecord {

        [Display(AutoGenerateField = false)]
        public ExposureTarget TargetUnit { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Code of the biological matrix.")]
        [DisplayName("Biological matrix code")]
        public string BiologicalMatrix { get; set; }

        [Description("The target unit of the concentration values.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Code of the sample type.")]
        [DisplayName("Sample type code")]
        public string SampleTypeCode { get; set; }

        [Description("Description, e.g. analytical method, sampling type.")]
        [DisplayName("Description")]
        public string Description { get; set; }

        [Description("Limit of reporting (LOR).")]
        [DisplayName("LOR")]
        public double LOR { get; set; }

        [Description("Number of measurements.")]
        [DisplayName("Number of measurements")]
        public int NumberOfMeasurements { get; set; }

        [Description("Number of positives.")]
        [DisplayName("Number of positives")]
        public int NumberOfPositives { get; set; }

        [Description("Percentage of positives.")]
        [DisplayName("Percentage")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> Percentiles { get; set; }

        [Description("p5.")]
        [DisplayName("p5")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double P5 { get { return Percentiles[0]; } }

        [Description("p10.")]
        [DisplayName("p10")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double P10 { get { return Percentiles[1]; } }

        [Description("p25.")]
        [DisplayName("p25")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double P25 { get { return Percentiles[2]; } }

        [Description("p50.")]
        [DisplayName("p50")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double P50 { get { return Percentiles[3]; } }

        [Description("p75.")]
        [DisplayName("p75")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double P75 { get { return Percentiles[4]; } }

        [Description("p90.")]
        [DisplayName("p90")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double P90 { get { return Percentiles[5]; } }

        [Description("p95.")]
        [DisplayName("p95")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double P95 { get { return Percentiles[6]; } }

        [Description("Mimimum of the positive values.")]
        [DisplayName("Min positives (HbmConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MinPositives { get; set; }

        [Description("Maximum of the positive values.")]
        [DisplayName("Max positives (HbmConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MaxPositives { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> Outliers { get; set; }

        [Description("Number of outliers.")]
        [DisplayName("Number of outliers")]
        public int NumberOfOutLiers { get; set; } = 0;
    }
}
