using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace MCRA.General {
    public enum MissingValueImputationMethod {
        [Description("Set missing values to zero.")]
        [Display(Name = "Set zero", ShortName = "Set zero")]
        SetZero = 0,
        [Description("Replace missing measurements by random other measurements of the same substance, biological matrix and sampling type.")]
        [Display(Name = "Impute from data", ShortName = "Impute from data")]
        ImputeFromData = 1,
        [Description("No missing value imputation, all missing values remain in the data set and samples with missing values will be removed before analysis.")]
        [Display(Name = "No missing value imputation", ShortName = "No imputation")]
        NoImputation = 2
    }
}
