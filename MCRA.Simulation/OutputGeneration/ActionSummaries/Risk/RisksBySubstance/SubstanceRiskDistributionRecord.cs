using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class SubstanceRiskDistributionRecord {

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Target biological matrix.")]
        [DisplayName("Biological matrix")]
        public string BiologicalMatrix { get; set; }

        [Description("Expression type.")]
        [DisplayName("Expression type")]
        public string ExpressionType { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsCumulativeRecord { get; set; }

        [Description("Lower (LowerConfidenceBound) percentile of the risk ({RiskMetric}) distribution (all values, nominal run).")]
        [DisplayName("Risk (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerRiskNom {
            get {
                return RiskPercentiles[0].ReferenceValue;
            }
        }

        [Description("Median of the uncertainty distribution of the lower (LowerConfidenceBound) percentile of the risk ({RiskMetric}) distribution (all values).")]
        [DisplayName("Risk (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerRiskUncP50 {
            get {
                return RiskPercentiles[0].MedianUncertainty;
            }
        }

        [Description("Median of the risk ({RiskMetric}) distribution (all values, nominal run).")]
        [DisplayName("Risk (median)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RiskP50Nom {
            get {
                return RiskPercentiles[1].ReferenceValue;
            }
        }

        [Description("Median of the uncertainty distribution of the median of the risk ({RiskMetric}) distribution (all values).")]
        [DisplayName("Risk (median)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RiskP50UncP50 {
            get {
                return RiskPercentiles[1].MedianUncertainty;
            }
        }

        [Description("Upper (UpperConfidenceBound) percentile of the risk ({RiskMetric}) distribution (all values, nominal run).")]
        [DisplayName("Risk (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperRiskNom {
            get {
                return RiskPercentiles[2].ReferenceValue;
            }
        }

        [Description("Median of the uncertainty distribution of the upper (UpperConfidenceBound) percentile of the risk ({RiskMetric}) distribution (all values).")]
        [DisplayName("Risk (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperRiskUncP50 {
            get {
                return RiskPercentiles[2].MedianUncertainty;
            }
        }

        [Description("Uncertainty lower bound (LowerBound) of the lower (LowerConfidenceBound) percentile of the risk distribution (all values).")]
        [DisplayName("Risk (LowerConfidenceBound) lower (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerRiskUncLower {
            get {
                return RiskPercentiles[0].Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper bound (UpperBound) of the upper (UpperConfidenceBound) percentile of the risk distribution (all values).")]
        [DisplayName("Risk (UpperConfidenceBound) upper (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperRiskUncUpper {
            get {
                return RiskPercentiles[2].Percentile(UncertaintyUpperLimit);
            }
        }

        [Description("Probability of critical exposure (%).")]
        [DisplayName("POCE (%)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ProbabilityOfCriticalEffect {
            get {
                return ProbabilityOfCriticalEffects[0].ReferenceValue;
            }
        }

        [Description("POCE (%) median uncertainty estimate.")]
        [DisplayName("POCE (%)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianProbabilityOfCriticalEffect { 
            get { 
                return ProbabilityOfCriticalEffects?[0].MedianUncertainty ?? double.NaN; 
            } 
        }

        [Description("POCE (%) lower ({LowerBound}) uncertainty bound.")]
        [DisplayName("POCE (%) lower ({LowerBound})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerProbabilityOfCriticalEffect { 
            get { 
                return ProbabilityOfCriticalEffects?[0].Percentile(UncertaintyLowerLimit) ?? double.NaN; 
            } 
        }

        [Description("POCE (%) upper ({UpperBound}) uncertainty bound.")]
        [DisplayName("POCE (%) upper ({UpperBound})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperProbabilityOfCriticalEffect { 
            get { 
                return ProbabilityOfCriticalEffects?[0].Percentile(UncertaintyUpperLimit) ?? double.NaN; 
            } 
        }

        [Description("Percentage positive exposure.")]
        [DisplayName("Percentage positive exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PercentagePositives { get; set; }

        [Display(AutoGenerateField = false)]
        public UncertainDataPointCollection<double> RiskPercentiles { get; set; }

        [Display(AutoGenerateField = false)]
        public UncertainDataPointCollection<double> ProbabilityOfCriticalEffects { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerLimit { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperLimit { get; set; }

    }
}
