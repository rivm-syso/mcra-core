using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class HazardIndexRecord {

        [DisplayName("Substance name")]
        public string CompoundName { get; set; }

        [DisplayName("Substance code")]
        public string CompoundCode { get; set; }

        [Display(AutoGenerateField = false)]
        public bool IsCumulativeRecord { get; set; }

        [Description("Lower percentile (LowerConfidenceBound) of the individual hazard index distribution (all values).")]
        [DisplayName("HI (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerHINom {
            get {
                return HazardIndexPercentiles[0].ReferenceValue;
            }
        }

        [Description("Median of the lower percentile (LowerConfidenceBound) of the individual hazard index distribution (all values).")]
        [DisplayName("HI (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerHIUncP50 {
            get {
                return HazardIndexPercentiles[0].MedianUncertainty;
            }
        }

        [Description("Hazard index.")]
        [DisplayName("HI")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double HIP50Nom {
            get {
                return HazardIndexPercentiles[1].ReferenceValue;
            }
        }

        [Description("Median of the p50 of the individual hazard index distribution (all values).")]
        [DisplayName("HI (p50)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double HIP50UncP50 {
            get {
                return HazardIndexPercentiles[1].MedianUncertainty;
            }
        }

        [Description("Upper percentile (UpperConfidenceBound) of the individual hazard index distribution (all values).")]
        [DisplayName("HI (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperHINom {
            get {
                return HazardIndexPercentiles[2].ReferenceValue;
            }
        }

        [Description("Median of the upper percentile (UpperConfidenceBound) of the individual hazard index distribution (all values).")]
        [DisplayName("HI (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperHIUncP50 {
            get {
                return HazardIndexPercentiles[2].MedianUncertainty;
            }
        }

        [Description("Uncertainty lower bound (LowerBound) of the lower percentile (LowerConfidenceBound) of the HI distribution (all values).")]
        [DisplayName("HI (LowerConfidenceBound) - Unc (LowerBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerHI_UncLower {
            get {
                return HazardIndexPercentiles[0].Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper bound (UpperBound) of the upper (UpperConfidenceBound) percentile of the HI distribution (all values).")]
        [DisplayName("HI (UpperConfidenceBound) - Unc (UpperBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperHI_UncUpper {
            get {
                return HazardIndexPercentiles[2].Percentile(UncertaintyUpperLimit);
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
        public UncertainDataPointCollection<double> HazardIndexPercentiles { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerLimit { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperLimit { get; set; }
        [Display(AutoGenerateField = false)]
        public UncertainDataPointCollection<double> ProbabilityOfCriticalEffects { get; set; }
    }
}
