using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum MissingValueImputationMethod {
        [Description("Set missing values to zero.")]
        [Display(Name = "Set zero", ShortName = "Set zero")]
        SetZero = 0,
        [Description("Replace missing measurements by random other measurements of the same substance, biological matrix and sampling type.")]
        [Display(Name = "Impute from data", ShortName = "Impute from data")]
        ImputeFromData = 1
    }
}
