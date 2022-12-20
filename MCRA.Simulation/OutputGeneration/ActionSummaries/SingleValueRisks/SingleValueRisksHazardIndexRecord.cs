using MCRA.Utils.Statistics;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueRisksHazardIndexRecord {




        [DisplayName("Reference substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Reference substance code")]
        public string SubstanceCode { get; set; }

        [Description("Hazard characterisation.")]
        [Display(Name = "Hazard characterisation")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double HazardCharacterisation { get; set; }

        [Description("Percentage to define a percentile of interest")]
        [Display(Name = "Percentage (%)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Percentage { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> ReferenceValueExposures { get; set; }

        [Description("Exposure.")]
        [DisplayName("Exposure")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double ReferenceValueExposure { get; set; }

        [Description("Median exposure.")]
        [Display(Name = "Median exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianExposure {
            get {
                return ReferenceValueExposures.Percentile(50);
            }
        }

        [Display(AutoGenerateField = false)]
        public List<double> HazardIndices { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> AdjustedHazardIndices { get; set; }

        [Description("Hazard index computed as the exposure divided by hazard characterisation (HI = EXP / HC).")]
        [Display(Name = "Unadjusted HI")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double HazardIndex { get; set; }

        [Description("Median hazard index computed as the exposure divided by hazard characterisation (HI = EXP / HC).")]
        [Display(Name = "Median unadjusted HI")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianHazardIndex {
            get {
                return HazardIndices.Percentile(50);
            }
        }

        [Description("Uncertainty lower bound HI (LowerConfidenceBound).")]
        [DisplayName("Unadjusted HI (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerHI_uncertainty {
            get {
                return HazardIndices.Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper bound HI (UpperConfidenceBound).")]
        [DisplayName("Unadjusted HI (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperHI_uncertainty {
            get {
                return HazardIndices.Percentile(UncertaintyUpperLimit);
            }
        }

        [Description("Adjusted hazard index computed as the exposure divided by hazard characterisation / adjustment factor (HI = EXP / HC).")]
        [Display(Name = "HI")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AdjustedHazardIndex {
            get {
                return HazardIndex / AdjustmentFactor;
            }
        }

        [Description("Median adjusted hazard index computed as the exposure divided by hazard characterisation / adjustment factor (HI = EXP / HC).")]
        [Display(Name = "Median HI")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAdjustedHazardIndex {
            get {
                return AdjustedHazardIndices.Percentile(50);
            }
        }

        [Description("Uncertainty lower bound adjusted HI (LowerConfidenceBound).")]
        [Display(Name = "HI (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerAdjustedHazardIndex {
            get {
                return AdjustedHazardIndices.Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper bound adjusted HI (UpperConfidenceBound).")]
        [Display(Name = "HI (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperAdjustedHazardIndex {
            get {
                return AdjustedHazardIndices.Percentile(UncertaintyUpperLimit);
            }
        }

        [Description("The product of the exposure and hazard related adjustment factors * background contribution to tail exposure.")]
        [Display(Name = "Overall adjustment factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AdjustmentFactor { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerLimit { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperLimit { get; set; }
    }
}
