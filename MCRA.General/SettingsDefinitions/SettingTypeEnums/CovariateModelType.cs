using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum CovariateModelType {
        [Display(Name = "Only constant")]
        [Description("No relation between exposure and e.g. age or gender.")]
        Constant,
        [Display(Name = "Only covariable")]
        [Description("Exposure depends on the covariable, e.g. age.")]
        Covariable,
        [Display(Name = "Only cofactor")]
        [Description("Exposure depends on the level of the cofactor, e.g. gender.")]
        Cofactor,
        [Display(Name = "Both covariable and cofactor")]
        [Description("Exposure depends on both covariable and cofactor (additive model).")]
        CovariableCofactor,
        [Display(Name = "Both covariable and cofactor and interaction")]
        [Description("Exposure depends on both covariable and cofactor and the effect of the covariable differs for different levels of the cofactor (multiplicative model).")]
        CovariableCofactorInteraction,
    }
}
