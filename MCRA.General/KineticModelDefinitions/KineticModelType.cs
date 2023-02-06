using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum KineticModelType {
        [Display(ShortName = "Undefined", Name = "Undefined")]
        Undefined = -1,
        [Display(ShortName = "EuroMix Generic PBTK model (v5)", Name = "EuroMix Generic PBTK model (v5)")]
        EuroMix_Generic_PBTK_model_V5 = 0,
        [Display(ShortName = "EuroMix Generic PBTK model (v6)", Name = "EuroMix Generic PBTK model (v6)")]
        EuroMix_Generic_PBTK_model_V6 = 1,
        [Display(ShortName = "EuroMix Bisphenols PBPK model (v1)", Name = "EuroMix Bisphenols PBPK model (v1)")]
        EuroMix_Bisphenols_PBPK_model_V1 = 2,
        [Display(ShortName = "EuroMix Bisphenols PBPK model (v2)", Name = "EuroMix Bisphenols PBPK model (v2)")]
        EuroMix_Bisphenols_PBPK_model_V2 = 3,
        [Display(ShortName = "PBK model chlorpyrifos (v1)", Name = "PBK model chlorpyrifos (v1)")]
        PBK_Chlorpyrifos_V1 = 4,
    }
}
