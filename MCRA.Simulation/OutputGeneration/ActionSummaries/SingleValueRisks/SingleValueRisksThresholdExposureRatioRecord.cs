using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueRisksThresholdExposureRatioRecord {

        [DisplayName("Reference substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Reference substance code")]
        public string SubstanceCode { get; set; }


        [Description("Hazard characterisation.")]
        [Display(Name = "Hazard characterisation")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double HazardCharacterisation { get; set; }

        [Description("Specified percentage exposure.")]
        [DisplayName("Percentage exposure (%)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double ExposurePercentage { get { return 100 - Percentage; } }

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

        [Description("Percentage")]
        [Display(Name = "Percentage MOE(T) (%)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Percentage { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> Risks { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> AdjustedRisks { get; set; }

        [Description("Unadjusted threshold value/exposure, computed as the hazard characterisation divided by the exposure (Threshold/Exp).")]
        [Display(Name = "Unadjusted MOE(T)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Risk { get; set; }

        [Description("Median threshold value/exposure computed as the hazard characterisation divided by the exposure (Threshold/Exp).")]
        [Display(Name = "Median unadjusted MOE(T)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianRisk {
            get {
                return Risks.Percentile(50);
            }
        }

        [Description("Uncertainty lower bound MOE(T) (LowerConfidenceBound).")]
        [DisplayName("Unadjusted MOE(T) (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerRisk_uncertainty {
            get {
                return Risks.Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper bound MOE(T) (UpperConfidenceBound).")]
        [DisplayName("Unadjusted MOE(T) (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperRisk_uncertainty {
            get {
                return Risks.Percentile(UncertaintyUpperLimit);
            }
        }

        [Description("Adjusted threshold value/exposure computed as the hazard characterisation divided by the exposure x adjustment factor (Threshold/Exp).")]
        [Display(Name = "MOE(T)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AdjustedRisk {
            get {
                return Risk * AdjustmentFactor;
            }
        }
        [Description("Median adjusted threshold value/exposure computed as the hazard characterisation divided by the exposure x adjustment factor (Threshold/Exp).")]
        [Display(Name = "Median MOE(T)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAdjustedRisk {
            get {
                return AdjustedRisks.Percentile(50);
            }
        }

        [Description("Uncertainty lower bound adjusted MOE(T)  (LowerConfidenceBound).")]
        [Display(Name = "MOE(T) (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerAdjustedRisk {
            get {
                return AdjustedRisks.Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper bound adjusted MOE(T) (UpperConfidenceBound).")]
        [Display(Name = "MOE(T) (UpperConfidenceBound)")]
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
