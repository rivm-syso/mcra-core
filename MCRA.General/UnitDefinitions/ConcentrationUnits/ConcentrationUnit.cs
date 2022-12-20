using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum ConcentrationUnit {
        [Display(ShortName = "kg/kg", Name = "kilogram/kilogram")]
        kgPerKg = 0,
        [Display(ShortName = "g/kg", Name = "gram/kilogram")]
        gPerKg = 1,
        [Display(ShortName = "mg/kg", Name = "milligram/kilogram")]
        mgPerKg = 2,
        [Display(ShortName = "µg/kg", Name = "microgram/kilogram")]
        ugPerKg = 3,
        [Display(ShortName = "ng/kg", Name = "nanogram/kilogram")]
        ngPerKg = 4,
        [Display(ShortName = "pg/kg", Name = "picogram/kilogram")]
        pgPerKg = 5,
        [Display(ShortName = "kg/L", Name = "kilogram/liter")]
        kgPerL = 6,
        [Display(ShortName = "g/L", Name = "gram/liter")]
        gPerL = 7,
        [Display(ShortName = "mg/L", Name = "milligram/liter")]
        mgPerL = 8,
        [Display(ShortName = "µg/L", Name = "microgram/liter")]
        ugPerL = 9,
        [Display(ShortName = "ng/L", Name = "nanogram/liter")]
        ngPerL = 10,
        [Display(ShortName = "pg/L", Name = "picogram/liter")]
        pgPerL = 11,
        [Display(ShortName = "µg/mL", Name = "microgram/milliliter")]
        ugPermL = 12,
        [Display(ShortName = "ng/mL", Name = "nanogram/milliliter")]
        ngPermL = 13,
    }
}
