using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class RelativePotencyFactorsSummaryRecord {

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerBound { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperBound { get; set; }

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Description("Relative Potency Factor (RPF) scaled such that the RPF of reference compound is equal to 1.")]
        [DisplayName("RPF (nominal) scaled to reference")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RelativePotencyFactor { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> RelativePotencyFactorUncertaintyValues { get; set; }

        [Description("Relative potency factor lower bound (LowerBound).")]
        [DisplayName("Relative potency factor lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RelativePotencyFactorLowerBoundPercentile {
            get {
                if (RelativePotencyFactorUncertaintyValues.Count > 1) {
                    return RelativePotencyFactorUncertaintyValues.Percentile(UncertaintyLowerBound);
                }
                return double.NaN;
            }
        }

        [Description("Relative potency factor upper bound (UpperBound).")]
        [DisplayName("Relative potency factor upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RelativePotencyFactorUpperBoundPercentile {
            get {
                if (RelativePotencyFactorUncertaintyValues.Count > 1) {
                    return RelativePotencyFactorUncertaintyValues.Percentile(UncertaintyUpperBound);
                }
                return double.NaN;
            }
        }
    }
}
