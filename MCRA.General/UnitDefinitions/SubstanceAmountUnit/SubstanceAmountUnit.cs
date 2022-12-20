using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum SubstanceAmountUnit {
        Undefined = -1,
        [Display(ShortName = "kg", Name = "kilograms")]
        Kilograms,
        [Display(ShortName = "g", Name = "grams")]
        Grams,
        [Display(ShortName = "mg", Name = "milligrams")]
        Milligrams,
        [Display(ShortName = "µg", Name = "micrograms")]
        Micrograms,
        [Display(ShortName = "ng", Name = "nanograms")]
        Nanograms,
        [Display(ShortName = "pg", Name = "picograms")]
        Picograms,
        [Display(ShortName = "fg", Name = "femtograms")]
        Femtograms,
        [Display(ShortName = "mol", Name = "moles")]
        Moles,
        [Display(ShortName = "mmol", Name = "millimoles")]
        Millimoles,
        [Display(ShortName = "µmol", Name = "micromoles")]
        Micromoles,
        [Display(ShortName = "nmol", Name = "nanomoles")]
        Nanomoles
    };
}
