using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Utils.Statistics;

namespace MCRA.Simulation.OutputGeneration {
    public class ConcentrationPercentileRecord {
        [Display(AutoGenerateField = false)]
        public double XValue { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerLimit { get; set; } = 2.5;

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperLimit { get; set; } = 97.5;

        [Display(AutoGenerateField = false)]
        public List<double> Values { get; set; }

        [Description("Specified percentage.")]
        [DisplayName("Percentage")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Percentage { get { return XValue * 100; } }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Concentration (ConcentrationUnit) at the specified percentage.")]
        [DisplayName("Concentration (ConcentrationUnit)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Value { get; set; }

        [Description("Median of the p50 of the uncertainty distribution of the concentration at the specified percentage.")]
        [DisplayName("Median (p50)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Median {
            get {
                return Values.Percentile(50);
            }
        }

        [Description("Lower bound (LowerBound) of the uncertainty distribution of the concentration at the specified percentage.")]
        [DisplayName("Uncertainty lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerBound {
            get {
                return Values.Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Upper bound (UpperBound) of the uncertainty distribution of the concentration at the specified percentage.")]
        [DisplayName("Uncertainty upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperBound {
            get {
                return Values.Percentile(UncertaintyUpperLimit);
            }
        }
    }
}
