using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueRisksAdjustmentFactorRecord {
        [Description("The exposure related adjustment factor for the selected percentile. For a distribution this is the median ")]
        [Display(Name = "Adjustment factor exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AdjustmentFactorExposure { get; set; }

        [Description("The hazard related adjustment factor for the selected percentile. For a distribution this is the median ")]
        [Display(Name = "Adjustment factor hazard")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AdjustmentFactorHazard { get; set; }

        [Description("The product of the exposure and hazard related adjustment factors for the selected risk percentile.")]
        [Display(Name = "Adjustment factor exposure * hazard")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AdjustmentFactorExposureHazard {
            get {
                return AdjustmentFactorExposure * AdjustmentFactorHazard;
            }
        }

        [Display(AutoGenerateField = false)]
        public double BackgroundContribution { get; set; }

        [Description("100 - focal contribution (= the contribution of the focal food and substance combination to the exposure of the upper tail).")]
        [Display(Name = "Background contribution to tail exposure (%)")]
        [DisplayFormat(DataFormatString = "{0:F1}")]
        public double BackgroundContributionPercentage {
            get {
                return BackgroundContribution * 100;
            }
        }

    [Description("The product of the exposure and hazard related adjustment factors * (1 - foreground contribution) + foreground contribution to tail exposure.")]
        [Display(Name = "Overall adjustment factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double AdjustmentFactor {
            get {
                return AdjustmentFactorExposure * AdjustmentFactorHazard * BackgroundContribution + (1 - BackgroundContribution);
            }
        }
    }
}
