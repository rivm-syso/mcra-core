using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum ConcentrationMassUnit {
        Undefined = -1,
        [Display(ShortName = "per unit", Name = "per unit")]
        PerUnit,
        [Display(ShortName = "kg", Name = "kilograms")]
        Kilograms,
        [Display(ShortName = "g", Name = "grams")]
        Grams,
        [Display(ShortName = "l", Name = "liter")]
        Liter,
        [Display(ShortName = "ml", Name = "milliliter")]
        Milliliter
    };
}
