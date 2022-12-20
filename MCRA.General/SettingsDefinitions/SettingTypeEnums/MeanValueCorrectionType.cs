using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum MeanValueCorrectionType {
        [Display(Name = "Unbiased")]
        [Description("The mean of the lognormal is unbiased (bias correction).")]
        Unbiased,
        [Display(Name = "Biased")]
        [Description("The mean of the lognormal is biased (no bias correction).")]
        Biased,
    }
}
