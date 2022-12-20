using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum DoseUnit {
        
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
        
        // Per day
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
        
        // Per kg
        [Display(ShortName = "kg/kg", Name = "kilogram/kilogram")]
        kgPerKg = 19,
        [Display(ShortName = "g/kg", Name = "gram/kilogram")]
        gPerKg = 20,
        [Display(ShortName = "mg/kg", Name = "milligram/kilogram")]
        mgPerKg = 21,
        [Display(ShortName = "µg/kg", Name = "microgram/kilogram")]
        ugPerKg = 22,
        [Display(ShortName = "ng/kg", Name = "nanogram/kilogram")]
        ngPerKg = 23,
        [Display(ShortName = "pg/kg", Name = "picogram/kilogram")]
        pgPerKg = 24,

        // Molar units
        [Display(ShortName = "M", Name = "molar")]
        M = 25,
        [Display(ShortName = "mM", Name = "millimolar")]
        mM = 26,
        [Display(ShortName = "µM", Name = "micromolar")]
        uM = 27,
        [Display(ShortName = "nM", Name = "nanomolar")]
        nM = 28,
        [Display(ShortName = "moles", Name = "moles")]
        moles = 29,
        [Display(ShortName = "mmoles", Name = "millimoles")]
        mmoles = 30,
        [Display(ShortName = "µmoles", Name = "micromoles")]
        umoles = 31,
        [Display(ShortName = "nmoles", Name = "nanomoles")]
        nmoles = 32,

        // Per kg bw per week
        [Display(ShortName = "g/kg bw/week", Name = "gram/kilogram bodyweight/week")]
        gPerKgBWPerWeek = 33,
        [Display(ShortName = "mg/kg bw/week", Name = "milligram/kilogram bodyweight/week")]
        mgPerKgBWPerWeek = 34,
        [Display(ShortName = "µg/kg bw/week", Name = "microgram/kilogram bodyweight/week")]
        ugPerKgBWPerWeek = 35,
        [Display(ShortName = "ng/kg bw/week", Name = "nanogram/kilogram bodyweight/week")]
        ngPerKgBWPerWeek = 36,
        [Display(ShortName = "pg/kg bw/week", Name = "picogram/kilogram bodyweight/week")]
        pgPerKgBWPerWeek = 37,
        [Display(ShortName = "fg/kg bw/week", Name = "femtogram/kilogram bodyweight/week")]
        fgPerKgBWPerWeek = 38,

        // Per g bw per week
        [Display(ShortName = "g/g bw/week", Name = "gram/gram bodyweight/week")]
        gPerGBWPerWeek = 39,
        [Display(ShortName = "mg/g bw/week", Name = "milligram/gram bodyweight/week")]
        mgPerGBWPerWeek = 40,
        [Display(ShortName = "µg/g bw/week", Name = "microgram/gram bodyweight/week")]
        ugPerGBWPerWeek = 41,
        [Display(ShortName = "ng/g bw/week", Name = "nanogram/gram bodyweight/week")]
        ngPerGBWPerWeek = 42,
        [Display(ShortName = "pg/g bw/week", Name = "picogram/gram bodyweight/week")]
        pgPerGBWPerWeek = 43,
        [Display(ShortName = "fg/g bw/week", Name = "femtogram/gram bodyweight/week")]
        fgPerGBWPerWeek = 44,

        // Per week
        [Display(ShortName = "kg/week", Name = "kilogram/week")]
        kgPerWeek = 45,
        [Display(ShortName = "g/week", Name = "gram/week")]
        gPerWeek = 46,
        [Display(ShortName = "mg/week", Name = "milligram/week")]
        mgPerWeek = 47,
        [Display(ShortName = "µg/week", Name = "microgram/week")]
        ugPerWeek = 48,
        [Display(ShortName = "ng/week", Name = "nanogram/week")]
        ngPerWeek = 49,
        [Display(ShortName = "pg/week", Name = "picogram/week")]
        pgPerWeek = 50,
        [Display(ShortName = "fg/week", Name = "femtogram/week")]
        fgPerWeek = 51,
    }
}
