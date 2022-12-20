using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum TargetDosesCalculationMethod {
        [Display(Name = "In-vivo PoDs (BMDs, NOAELs, etc.)")]
        InVivoPods,
        [Display(Name = "In-vitro BMDs")]
        InVitroBmds,
        [Display(Name = "In-vivo PoDs for index substance, others using RPFs from in-vitro dose response models")]
        CombineInVivoPodInVitroDrms,
    };
}
