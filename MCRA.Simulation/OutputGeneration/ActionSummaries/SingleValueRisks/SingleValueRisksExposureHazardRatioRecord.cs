using MCRA.Utils.Statistics;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueRisksExposureHazardRatioRecord {

        [DisplayName("Reference substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Reference substance code")]
        public string SubstanceCode { get; set; }

        [Description("The threshold dose (in {HazardCharacterisationsUnit}) for the substance considered for the selected health effect.")]
        [Display(Name = "Hazard characterisation ({HazardCharacterisationsUnit})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double HazardCharacterisation { get; set; }

        [Description("Specified percentage of risk distribution.")]
        [DisplayName("Percentage (%)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Percentage { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> ReferenceValueExposures { get; set; }

        [Description("Estimate of the substance exposure through the specified route/source (in {ExposuresUnit}).")]
        [DisplayName("Exposure ({ExposuresUnit})")]
        [DisplayFormat(DataFormatString = "{0:G4}")]
        public double ReferenceValueExposure { get; set; }

        [Description("Median exposure.")]
        [Display(Name = "Median exposure ({ExposuresUnit})")]
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

        [Description("Risk ({RiskMetric}) (nominal run).")]
        [Display(Name = "Risk ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Risk { get; set; }

        [Description("Median risk ({RiskMetric}) of uncertainty analysis cycles.")]
        [Display(Name = "Median risk ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianRisk {
            get {
                return Risks.Percentile(50);
            }
        }

        [Description("Uncertainty lower (LowerConfidenceBound) bound of the risk ({RiskMetric}).")]
        [DisplayName("Risk ({RiskMetricShort}) (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerRisk_uncertainty {
            get {
                return Risks.Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper (UpperConfidenceBound) bound of the risk ({RiskMetric}).")]
        [DisplayName("Risk ({RiskMetricShort}) (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperRisk_uncertainty {
            get {
                return Risks.Percentile(UncertaintyUpperLimit);
            }
        }

        [Description("Adjusted risk ({RiskMetric}) computed as risk x adjustment factor.")]
        [Display(Name = "Adjusted risk ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AdjustedRisk {
            get {
                return Risk / AdjustmentFactor;
            }
        }

        [Description("Median of the adjusted risk ({RiskMetric}) of uncertainty analysis cycles.")]
        [Display(Name = "Median adjusted risk ({RiskMetricShort})")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAdjustedRisk {
            get {
                return AdjustedRisks.Percentile(50);
            }
        }

        [Description("Uncertainty lower (LowerConfidenceBound) bound of the adjusted risk ({RiskMetric}).")]
        [Display(Name = "Adjusted risk ({RiskMetricShort}) (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerAdjustedRisk {
            get {
                return AdjustedRisks.Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper (UpperConfidenceBound) bound of the adjusted risk ({RiskMetric}).")]
        [Display(Name = "Adjusted risk ({RiskMetricShort}) (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperAdjustedRisk {
            get {
                return AdjustedRisks.Percentile(UncertaintyUpperLimit);
            }
        }

        [Description("The product of the exposure and hazard related adjustment factors multiplied by the background contribution to tail exposure.")]
        [Display(Name = "Overall adjustment factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AdjustmentFactor { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyLowerLimit { get; set; }

        [Display(AutoGenerateField = false)]
        public double UncertaintyUpperLimit { get; set; }
    }
}
