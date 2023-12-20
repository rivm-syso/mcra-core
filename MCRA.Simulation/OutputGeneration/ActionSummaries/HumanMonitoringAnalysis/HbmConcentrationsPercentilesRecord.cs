using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData {
    public sealed class HbmConcentrationsPercentilesRecord {

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Description, e.g. analytical method, sampling type.")]
        [DisplayName("Description")]
        public string Description { get; set; }

        [Description("Unit.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Maximum of the positive values.")]
        [DisplayName("Max. positives")]
        public double MaxPositives { get; set; }

        [Description("Mimimum of the positive values.")]
        [DisplayName("Min. positives")]
        public double MinPositives { get; set; }

        [Description("p5.")]
        [DisplayName("p5")]
        public double P5 { get { return Percentiles[0]; } }

        [Description("p10.")]
        [DisplayName("p10")]
        public double P10 { get { return Percentiles[1]; } }

        [Description("p25.")]
        [DisplayName("p25")]
        public double P25 { get { return Percentiles[2]; } }

        [Description("p50.")]
        [DisplayName("p50")]
        public double P50 { get { return Percentiles[3]; } }

        [Description("p75.")]
        [DisplayName("p75")]
        public double P75 { get { return Percentiles[4]; } }

        [Description("p90.")]
        [DisplayName("p90")]
        public double P90 { get { return Percentiles[5]; } }

        [Description("p95.")]
        [DisplayName("p95")]
        public double P95 { get { return Percentiles[6]; } }

        [Description("Number of positives.")]
        [DisplayName("Number of positives")]
        public int NumberOfPositives { get; set; }

        [Description("Percentage of positives.")]
        [DisplayName("Percentage")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double Percentage { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> Percentiles { get; set; }
    }
}
