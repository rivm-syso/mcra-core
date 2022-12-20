using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum ExposureUnit {
        // Per kg bw per day
        [Display(ShortName = "g/kg bw/day", Name = "gram/kilogram bodyweight/day")]
        gPerKgBWPerDay = 0,
        [Display(ShortName = "mg/kg bw/day", Name = "milligram/kilogram bodyweight/day")]
        mgPerKgBWPerDay = 1,
        [Display(ShortName = "µg/kg bw/day", Name = "microgram/kilogram bodyweight/day")]
        ugPerKgBWPerDay = 2,
        [Display(ShortName = "ng/kg bw/day", Name = "nanogram/kilogram bodyweight/day")]
        ngPerKgBWPerDay = 3,
        [Display(ShortName = "pg/kg bw/day", Name = "picogram/kilogram bodyweight/day")]
        pgPerKgBWPerDay = 4,
        [Display(ShortName = "fg/kg bw/day", Name = "femtogram/kilogram bodyweight/day")]
        fgPerKgBWPerDay = 5,
        // Per g bw per day
        [Display(ShortName = "g/g bw/day", Name = "gram/gram bodyweight/day")]
        gPerGBWPerDay = 6,
        [Display(ShortName = "mg/g bw/day", Name = "milligram/gram bodyweight/day")]
        mgPerGBWPerDay = 7,
        [Display(ShortName = "µg/g bw/day", Name = "microgram/gram bodyweight/day")]
        ugPerGBWPerDay = 8,
        [Display(ShortName = "ng/g bw/day", Name = "nanogram/gram bodyweight/day")]
        ngPerGBWPerDay = 9,
        [Display(ShortName = "pg/g bw/day", Name = "picogram/gram bodyweight/day")]
        pgPerGBWPerDay = 10,
        [Display(ShortName = "fg/g bw/day", Name = "femtogram/gram bodyweight/day")]
        fgPerGBWPerDay = 11,
        // Per day (per person/compartment)
        [Display(ShortName = "kg/day", Name = "kilogram/day")]
        kgPerDay = 12,
        [Display(ShortName = "g/day", Name = "gram/day")]
        gPerDay = 13,
        [Display(ShortName = "mg/day", Name = "milligram/day")]
        mgPerDay = 14,
        [Display(ShortName = "µg/day", Name = "microgram/day")]
        ugPerDay = 15,
        [Display(ShortName = "ng/day", Name = "nanogram/day")]
        ngPerDay = 16,
        [Display(ShortName = "pg/day", Name = "picogram/day")]
        pgPerDay = 17,
        [Display(ShortName = "fg/day", Name = "femtogram/day")]
        fgPerDay = 18,
        // exposure per kg at internal level (target exposures, no day)
        [Display(ShortName = "g/kg", Name = "gram/kilogram")]
        gPerKg = 19,
        [Display(ShortName = "mg/kg", Name = "milligram/kilogram")]
        mgPerKg = 20,
        [Display(ShortName = "µg/kg", Name = "microgram/kilogram")]
        ugPerKg = 21,
        [Display(ShortName = "ng/kg", Name = "nanogram/kilogram")]
        ngPerKg = 22,
        [Display(ShortName = "pg/kg", Name = "picogram/kilogram")]
        pgPerKg = 23,
        [Display(ShortName = "fg/kg", Name = "femtogram/kilogram")]
        fgPerKg = 24,
        // exposure (per person/compartment) at internal level (target exposures, no day)
        [Display(ShortName = "g", Name = "gram")]
        g = 25,
        [Display(ShortName = "mg", Name = "milligram")]
        mg = 26,
        [Display(ShortName = "µg", Name = "microgram")]
        ug = 27,
        [Display(ShortName = "ng", Name = "nanogram")]
        ng = 28,
        [Display(ShortName = "pg", Name = "picogram")]
        pg = 29,
        [Display(ShortName = "fg", Name = "femtogram")]
        fg = 30,
    }
}
