using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration.CombinedActionSummaries {
    public class CombinedPercentileRecord {
        [Display(AutoGenerateField = false)]
        public string IdModel { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> UncertaintyValues { get; set; }

        [Description("The country and population of the consumption data (survey).")]
        [DisplayName("Population")]
        public string Name { get; set; }

        [Description("Specified percentage (%).")]
        [DisplayName("Percentage (%)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Percentage { get; set; }

        [Description("The nominal value at the specified percentage.")]
        [DisplayName("Nominal value")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Value { get; set; }

        [Description("Median (p50) of the uncertainty distribution of the value at the specified percentage.")]
        [DisplayName("Median (p50)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? UncertaintyMedian { get; set; }

        [Description("Lower bound (LowerBound) of the uncertainty distribution of the value at the specified percentage.")]
        [DisplayName("Lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? UncertaintyLowerBound { get; set; }

        [Description("Upper bound (UpperBound) of the uncertainty distribution of the value at the specified percentage.")]
        [DisplayName("Upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? UncertaintyUpperBound { get; set; }

        [Display(AutoGenerateField = false)]
        public bool HasUncertainty =>
            UncertaintyMedian != null
                && UncertaintyLowerBound != null
                && UncertaintyUpperBound != null
                && !double.IsNaN((double)UncertaintyMedian)
                && (!double.IsNaN((double)UncertaintyLowerBound)
                    || !double.IsNaN((double)UncertaintyUpperBound));
    }
}
