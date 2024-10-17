using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MathNet.Numerics.Statistics;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.Risk {
    public sealed class PercentageAtRiskRecord {

        [Display(AutoGenerateField = false)]
        public List<double> Percentages { get; set; }

        [Description("Mean contribution (%) for individuals to a substance.")]
        [DisplayName("Calculated percentage (%)")]
        public double Percentage { get; set; }

        [Description("Median contribution (%) for individuals to a substance.")]
        [DisplayName("Calculated percentage (%) median")]
        public double MedianContribution { get { return Percentages.Any() ? Percentages.Percentile(50) : double.NaN; } }
    }
}
