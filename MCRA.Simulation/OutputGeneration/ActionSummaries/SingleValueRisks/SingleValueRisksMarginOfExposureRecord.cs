using MCRA.Utils.Statistics;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueRisksMarginOfExposureRecord {

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
        [Display(Name = "Percentage MOE (%)")]
        [DisplayFormat(DataFormatString = "{0:F2}")]
        public double Percentage { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> MarginOfExposures { get; set; }

        [Display(AutoGenerateField = false)]
        public List<double> AdjustedMarginOfExposures { get; set; }

        [Description("Unadjusted margin of exposure computed as the hazard characterisation divided by the exposure (MOE = HC / EXP).")]
        [Display(Name = "Unadjusted MOE")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MarginOfExposure { get; set; }

        [Description("Median margin of exposure computed as the hazard characterisation divided by the exposure (MOE = HC / EXP).")]
        [Display(Name = "Median unadjusted MOE")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianMarginOfExposure {
            get {
                return MarginOfExposures.Percentile(50);
            }
        }

        [Description("Uncertainty lower bound MOE (LowerConfidenceBound).")]
        [DisplayName("Unadjusted MOE (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerMOE_uncertainty {
            get {
                return MarginOfExposures.Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper bound MOE (UpperConfidenceBound).")]
        [DisplayName("Unadjusted MOE (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperMOE_uncertainty {
            get {
                return MarginOfExposures.Percentile(UncertaintyUpperLimit);
            }
        }

        [Description("Adjusted margin of exposure computed as the hazard characterisation divided by the exposure x adjustment factor (MOE = HC / EXP).")]
        [Display(Name = "MOE")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AdjustedMarginOfExposure {
            get {
                return MarginOfExposure * AdjustmentFactor;
            }
        }
        [Description("Median adjusted margin of exposure computed as the hazard characterisation divided by the exposure x adjustment factor (MOE = HC / EXP).")]
        [Display(Name = "Median MOE")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double MedianAdjustedMarginOfExposure {
            get {
                return AdjustedMarginOfExposures.Percentile(50);
            }
        }

        [Description("Uncertainty lower bound adjusted MOE  (LowerConfidenceBound).")]
        [Display(Name = "MOE (LowerConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PLowerAdjustedMarginOfExposure {
            get {
                return AdjustedMarginOfExposures.Percentile(UncertaintyLowerLimit);
            }
        }

        [Description("Uncertainty upper bound adjusted MOE (UpperConfidenceBound).")]
        [Display(Name = "MOE (UpperConfidenceBound)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double PUpperAdjustedMarginOfExposure {
            get {
                return AdjustedMarginOfExposures.Percentile(UncertaintyUpperLimit);
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
