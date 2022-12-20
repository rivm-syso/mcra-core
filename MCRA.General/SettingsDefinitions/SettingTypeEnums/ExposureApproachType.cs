using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum ExposureApproachType {
        [Display(Name = "Risk based (RPFs)")]
        [Description("Exposures are multiplied by the RPF and thus exposures to the different substances are on the same and comparable scale.")]
        RiskBased,
        [Display(Name = "Standardised")]
        [Description("All substances are standardised to equal variance, and the selection of the components will work on patterns of correlation only.")]
        ExposureBased
    }
}
