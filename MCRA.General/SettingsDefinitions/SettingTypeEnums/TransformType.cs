using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum TransformType {
        [Display(Name = "Logarithmic")]
        [Description("Exposure amounts are transformed to normality using a logarithmic transformation.")]
        Logarithmic,
        [Display(Name = "No transformation")]
        [Description("Exposure amounts are not transformed.")]
        NoTransform,
        [Display(Name = "Power")]
        [Description("Exposure amounts are transformed to normality using a Box-Cox power transformation.")]
        Power,
    }
}
