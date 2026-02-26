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
        [Display(ShortName = "mg", Name = "milligrams")]
        MilliGrams,
        [Display(ShortName = "L", Name = "liter")]
        Liter,
        [Display(ShortName = "mL", Name = "milliliter")]
        Milliliter,
        [Display(ShortName = "dL", Name = "deciliter")]
        Deciliter,
        [Display(ShortName = "cL", Name = "centiliter")]
        Centiliter,
    };
}
