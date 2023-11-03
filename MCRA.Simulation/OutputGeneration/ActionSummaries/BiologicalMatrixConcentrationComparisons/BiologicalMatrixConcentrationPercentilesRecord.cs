using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData {
    public sealed class BiologicalMatrixConcentrationPercentilesRecord {

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Type of the concentration estimate (modelled/monitoring)")]
        [DisplayName("Type")]
        public string Type { get; set; }

        [Description("The biological matrix at which the concentrations were observed/modelled.")]
        [DisplayName("Biological matrix")]
        public string BiologicalMatrix { get; set; }

        [Description("The target unit of the concentration values.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Exposure route")]
        [DisplayName("Exposure Route")]
        public string ExposureRoute { get; set; }

        [DisplayName("Sampling type")]
        [Description("Sampling type used to obtain the concentrations.")]
        [Display(AutoGenerateField = false)]
        public string SamplingType { get; set; }

        [Description("Number of {IndividualDayUnit} days with concentrations > 0 (not corrected for sampling weights).")]
        [DisplayName("{IndividualDayUnit} with positive concentrations")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfPositives { get; set; }

        [Description("Percentage of {IndividualDayUnit} with concentrations value > 0 (if applicable, corrected for sampling weights).")]
        [DisplayName("Percentage {IndividualDayUnit} with positive concentrations")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double PercentagePositives { get; set; }

        [Description("Mean measurement value of all {IndividualDayUnit} (corrected for specific gravity correction factor).")]
        [DisplayName("Mean all {IndividualDayUnit}")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Total { get { return MeanPositives * PercentagePositives / 100; } }

        [Description("Median of measurement values of all {IndividualDayUnit} (corrected for specific gravity correction factor).")]
        [DisplayName("Median all {IndividualDayUnit}")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAll { get; set; }

        [Description("Lower percentile point of measurement values of all {IndividualDayUnit} (corrected for specific gravity correction factor).")]
        [DisplayName("{LowerPercentage} all {IndividualDayUnit}")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentileAll { get; set; }

        [Description("Upper percentile point of measurement values of all {IndividualDayUnit} (corrected for specific gravity correction factor).")]
        [DisplayName("{UpperPercentage} all {IndividualDayUnit}")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentileAll { get; set; }

        [Description("Average of the concentration values > 0 (corrected for specific gravity correction factor).")]
        [DisplayName("Mean {IndividualDayUnit} positive concentrations")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MeanPositives { get; set; }

        [Description("Median of the concentration values > 0 (corrected for specific gravity correction factor).")]
        [DisplayName("Median {IndividualDayUnit} positive concentrations")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianPositives { get; set; }

        [Description("Lower percentile point of the concentration values > 0 (corrected for specific gravity correction factor).")]
        [DisplayName("{LowerPercentage} {IndividualDayUnit} positive concentrations")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerPercentilePositives { get; set; }

        [Description("Upper percentile point of the concentration values > 0 (corrected for specific gravity correction factor).")]
        [DisplayName("{UpperPercentage} {IndividualDayUnit} positive concentrations")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperPercentilePositives { get; set; }

        [Description("Mimimum of the positive values.")]
        [DisplayName("Min. positives")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MinPositives { get; set; }

        [Description("Maximum of the positive values.")]
        [DisplayName("Max. positives")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MaxPositives { get; set; }

        [Description("p5 percentile point of the concentration distribution.")]
        [DisplayName("p5")]
        [Display(AutoGenerateField = false)]
        public double P5 { get { return BoxPlotPercentiles[0]; } }

        [Description("p10 percentile point of the concentration distribution.")]
        [DisplayName("p10")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(AutoGenerateField = false)]
        public double P10 { get { return BoxPlotPercentiles[1]; } }

        [Description("p25 percentile point of the concentration distribution.")]
        [DisplayName("p25")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(AutoGenerateField = false)]
        public double P25 { get { return BoxPlotPercentiles[2]; } }

        [Description("p50 percentile point of the concentration distribution.")]
        [DisplayName("p50")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(AutoGenerateField = false)]
        public double P50 { get { return BoxPlotPercentiles[3]; } }

        [Description("p75 percentile point of the concentration distribution.")]
        [DisplayName("p75")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(AutoGenerateField = false)]
        public double P75 { get { return BoxPlotPercentiles[4]; } }

        [Description("p90 percentile point of the concentration distribution.")]
        [DisplayName("p90")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(AutoGenerateField = false)]
        public double P90 { get { return BoxPlotPercentiles[5]; } }

        [Description("p95 percentile point of the concentration distribution.")]
        [DisplayName("p95")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        [Display(AutoGenerateField = false)]
        public double P95 { get { return BoxPlotPercentiles[6]; } }

        [Display(AutoGenerateField = false)]
        public List<double> BoxPlotPercentiles { get; set; }
    }
}
