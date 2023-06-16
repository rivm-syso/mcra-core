using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueExposureThresholdRatioIndexRecord {

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
        public List<double> Risks { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> AdjustedRisks { get; set; }

        [Description("Exposure/threshold value computed as the exposure divided by hazard characterisation (Exp/Threshold).")]
        [Display(Name = "Unadjusted Exp/Threshold")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Risk { get; set; }

        [Description("Median exposure/threshold value computed as the exposure divided by hazard characterisation (Exp/Threshold).")]
        [Display(Name = "Median unadjusted Exp/Threshold")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianRisk {
            get {
                return Risks.Percentile(50);
            }
        }

        [Description("Uncertainty lower bound Exp/Threshold (LowerConfidenceBound).")]
        [DisplayName("Unadjusted Exp/Threshold (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerRisk_uncertainty {
            get {
                return Risks.Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper bound Exp/Threshold (UpperConfidenceBound).")]
        [DisplayName("Unadjusted Exp/Threshold (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperRisk_uncertainty {
            get {
                return Risks.Percentile(UncertaintyUpperLimit);
            }
        }

        [Description("Adjusted exposure/threshold value computed as the exposure divided by hazard characterisation / adjustment factor (Exp/Threshold).")]
        [Display(Name = "Exp/Threshold")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AdjustedRisk {
            get {
                return Risk / AdjustmentFactor;
            }
        }

        [Description("Median adjusted exposure/threshold value computed as the exposure divided by hazard characterisation / adjustment factor (Exp/Threshold).")]
        [Display(Name = "Median Exp/Threshold")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAdjustedRisk {
            get {
                return AdjustedRisks.Percentile(50);
            }
        }

        [Description("Uncertainty lower bound adjusted Exp/Threshold (LowerConfidenceBound).")]
        [Display(Name = "Exp/Threshold (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerAdjustedRisk {
            get {
                return AdjustedRisks.Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper bound adjusted Exp/Threshold (UpperConfidenceBound).")]
        [Display(Name = "Exp/Threshold (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperAdjustedRisk {
            get {
                return AdjustedRisks.Percentile(UncertaintyUpperLimit);
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
