using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class ExposureThresholdRatioRecord {

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsCumulativeRecord { get; set; }

        [Description("Lower percentile (LowerConfidenceBound) of the individual risk (exposure/threshold value) distribution (all values).")]
        [DisplayName("Exp/Threshold (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerRiskNom {
            get {
                return ExposureRisks[0].ReferenceValue;
            }
        }

        [Description("Median of the lower percentile (LowerConfidenceBound) of the individual risk (exposure/threshold value) distribution (all values).")]
        [DisplayName("Exp/Threshold (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerRiskUncP50 {
            get {
                return ExposureRisks[0].MedianUncertainty;
            }
        }

        [Description("Exposure/threshold value.")]
        [DisplayName("Exp/Threshold")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RiskP50Nom {
            get {
                return ExposureRisks[1].ReferenceValue;
            }
        }

        [Description("Median of the p50 of the individual risk (exposure/threshold value) distribution (all values).")]
        [DisplayName("Exp/Threshold (p50)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RiskP50UncP50 {
            get {
                return ExposureRisks[1].MedianUncertainty;
            }
        }

        [Description("Upper percentile (UpperConfidenceBound) of the individual risk (exposure/threshold value) distribution (all values).")]
        [DisplayName("Exp/Threshold (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperRiskNom {
            get {
                return ExposureRisks[2].ReferenceValue;
            }
        }

        [Description("Median of the upper percentile (UpperConfidenceBound) of the individual risk (exposure/threshold value) distribution (all values).")]
        [DisplayName("Exp/Threshold (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperRiskUncP50 {
            get {
                return ExposureRisks[2].MedianUncertainty;
            }
        }

        [Description("Uncertainty lower bound (LowerBound) of the lower percentile (LowerConfidenceBound) of the Exp/Threshold distribution (all values).")]
        [DisplayName("Exp/Threshold (LowerConfidenceBound) - Unc (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerRisk_UncLower {
            get {
                return ExposureRisks[0].Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper bound (UpperBound) of the upper (UpperConfidenceBound) percentile of the Exp/Threshold distribution (all values).")]
        [DisplayName("Exp/Threshold (UpperConfidenceBound) - Unc (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperRisk_UncUpper {
            get {
                return ExposureRisks[2].Percentile(UncertaintyUpperLimit);
            }
        }

        [Description("Probability of critical exposure.")]
        [DisplayName("POCE (%)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ProbabilityOfCriticalEffect {
            get {
                return ProbabilityOfCriticalEffects[0].ReferenceValue;
            }
        }

        [Description("Median Probability of critical exposure (POCE) (%).")]
        [DisplayName("POCE (%)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianProbabilityOfCriticalEffect { get { return ProbabilityOfCriticalEffects?[0].MedianUncertainty ?? double.NaN; } }


        [Description("Lower uncertainty bound POCE (%).")]
        [DisplayName("POCE (%) lower bound (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double LowerProbabilityOfCriticalEffect { get { return ProbabilityOfCriticalEffects?[0].Percentile(UncertaintyLowerLimit) ?? double.NaN; } }

        [Description("Upper uncertainty bound POCE (%).")]
        [DisplayName("POCE (%) upper bound (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double UpperProbabilityOfCriticalEffect { get { return ProbabilityOfCriticalEffects?[0].Percentile(UncertaintyUpperLimit) ?? double.NaN; } }

        [Description("Percentage positive exposure.")]
        [DisplayName("Percentage positive exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PercentagePositives { get; set; }

        [Display(AutoGenerateField = false)]
        public UncertainDataPointCollection<double> ExposureRisks { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerLimit { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperLimit { get; set; }
        [Display(AutoGenerateField = false)]
        public UncertainDataPointCollection<double> ProbabilityOfCriticalEffects { get; set; }
    }
}
