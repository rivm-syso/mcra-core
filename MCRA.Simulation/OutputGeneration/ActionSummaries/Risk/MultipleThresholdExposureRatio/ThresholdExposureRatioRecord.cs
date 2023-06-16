using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class ThresholdExposureRatioRecord {

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsCumulativeRecord { get; set; }

        [Description("Lower percentile (LowerConfidenceBound) of the individual risk (threshold value/exposure) distribution.")]
        [DisplayName("Threshold/Exp (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerRiskNom {
            get {
                return ThresholdExposureRatioPercentiles[0].ReferenceValue;
            }
        }

        [Description("Median of the lower percentile (LowerConfidenceBound) of the individual risk (threshold value/exposure) distribution.")]
        [DisplayName("Threshold/Exp (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerRiskUncP50 {
            get {
                return ThresholdExposureRatioPercentiles[0].MedianUncertainty;
            }
        }

        [Description("Threshold value/exposure.")]
        [DisplayName("Threshold/Exp")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RiskP50Nom {
            get {
                return ThresholdExposureRatioPercentiles[1].ReferenceValue;
            }
        }
        [Description("Median of the p50 of the individual risk (threshold value/exposure) distribution.")]
        [DisplayName("Threshold/Exp (p50)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double RiskP50UncP50 {
            get {
                return ThresholdExposureRatioPercentiles[1].MedianUncertainty;
            }
        }

        [Description("Upper percentile (UpperConfidenceBound) of the individual risk (threshold value/exposure) distribution.")]
        [DisplayName("Threshold/Exp (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperRiskNom {
            get {
                return ThresholdExposureRatioPercentiles[2].ReferenceValue;
            }
        }

        [Description("Median of the upper percentile (UpperConfidenceBound) of the individual risk (threshold value/exposure) distribution.")]
        [DisplayName("MOE(T) (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperRiskUncP50 {
            get {
                return ThresholdExposureRatioPercentiles[2].MedianUncertainty;
            }
        }

        [Description("Uncertainty lower bound (LowerBound) of the lower percentile (LowerConfidenceBound) of the MOE(T) distribution.")]
        [DisplayName("MOE(T) (LowerConfidenceBound) - Unc (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerRiskUncLower {
            get {
                return ThresholdExposureRatioPercentiles[0].Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper bound (UpperBound) of the upper (UpperConfidenceBound) percentile of the MOE(T) distribution.")]
        [DisplayName("MOE(T) (UpperConfidenceBound) - Unc (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperRiskUncUpper {
            get {
                return ThresholdExposureRatioPercentiles[2].Percentile(UncertaintyUpperLimit);
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
        public UncertainDataPointCollection<double> ThresholdExposureRatioPercentiles { get; set; }


        [Display(AutoGenerateField = false)]
        public UncertainDataPointCollection<double> ProbabilityOfCriticalEffects { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerLimit { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperLimit { get; set; }

    }
}
