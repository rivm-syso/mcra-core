using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class MarginOfExposureRecord {

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsCumulativeRecord { get; set; }

        [Description("Lower percentile (LowerConfidenceBound) of the individual margin of exposure distribution.")]
        [DisplayName("MOE (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerMOENom {
            get {
                return MarginOfExposurePercentiles[0].ReferenceValue;
            }
        }

        [Description("Median of the lower percentile (LowerConfidenceBound) of the individual margin of exposure distribution.")]
        [DisplayName("MOE (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerMOEUncP50 {
            get {
                return MarginOfExposurePercentiles[0].MedianUncertainty;
            }
        }

        [Description("Margin of exposure.")]
        [DisplayName("MOE")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MOEP50Nom {
            get {
                return MarginOfExposurePercentiles[1].ReferenceValue;
            }
        }
        [Description("Median of the p50 of the individual margin of exposure distribution.")]
        [DisplayName("MOE (p50)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MOEP50UncP50 {
            get {
                return MarginOfExposurePercentiles[1].MedianUncertainty;
            }
        }

        [Description("Upper percentile (UpperConfidenceBound) of the individual margin of exposure distribution.")]
        [DisplayName("MOE (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperMOENom {
            get {
                return MarginOfExposurePercentiles[2].ReferenceValue;
            }
        }

        [Description("Median of the upper percentile (UpperConfidenceBound) of the individual margin of exposure distribution.")]
        [DisplayName("MOE (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperMOEUncP50 {
            get {
                return MarginOfExposurePercentiles[2].MedianUncertainty;
            }
        }

        [Description("Uncertainty lower bound (LowerBound) of the lower percentile (LowerConfidenceBound) of the MOE distribution.")]
        [DisplayName("MOE (LowerConfidenceBound) - Unc (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerMOE_UncLower {
            get {
                return MarginOfExposurePercentiles[0].Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper bound (UpperBound) of the upper (UpperConfidenceBound) percentile of the MOE distribution.")]
        [DisplayName("MOE (UpperConfidenceBound) - Unc (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperMOE_UncUpper {
            get {
                return MarginOfExposurePercentiles[2].Percentile(UncertaintyUpperLimit);
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
        public UncertainDataPointCollection<double> MarginOfExposurePercentiles { get; set; }


        [Display(AutoGenerateField = false)]
        public UncertainDataPointCollection<double> ProbabilityOfCriticalEffects { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerLimit { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperLimit { get; set; }

    }
}
