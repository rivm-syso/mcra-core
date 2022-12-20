using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum ConsumptionIntakeUnit {
        [Display(ShortName = "g/kg bw/day", Name = "gram/kilogram bodyweight/day")]
        gPerKgBWPerDay,
        [Display(ShortName = "g/day", Name = "gram/day")]
        gPerDay,
    }
}
