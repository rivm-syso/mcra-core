using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum HealthEffectType {
        [Display(Name = "Risk")]
        [Description("Health effect is negative (risk).")]
        Risk,
        [Display(Name = "Benefit")]
        [Description("Health effect is positive (benefit).")]
        Benefit
    }
}
