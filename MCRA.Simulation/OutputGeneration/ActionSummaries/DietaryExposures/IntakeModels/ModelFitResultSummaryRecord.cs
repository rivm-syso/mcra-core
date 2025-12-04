using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace MCRA.Simulation.OutputGeneration {
    public class ModelFitResultSummaryRecord {

        [Description("Parameter.")]
        [DisplayName("Parameter")]
        public string Parameter { get; set; }

        [Description("Estimate.")]
        [DisplayName("Estimate")]
        [DisplayFormat(DataFormatString = "{0:0.##}")]
        public double Estimate { get; set; }

        [Description("Standard error.")]
        [DisplayName("s.e.")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double? StandardError { get; set; }

        [Description("t-value.")]
        [DisplayName("t-value")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double? TValue { get; set; }
    }
}