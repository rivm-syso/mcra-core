using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData {
    public sealed class ComponentClusterPercentilesRecord {
        [Description("Component.")]
        [DisplayName("Component")]
        public int ComponentNumber { get; set; }

        [Description("Subgroup")]
        [DisplayName("Subgroup")]
        public int ClusterId { get; set; }

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

        [Display(AutoGenerateField = false)]
        public List<double> Percentiles { get; set; }
    }
}
