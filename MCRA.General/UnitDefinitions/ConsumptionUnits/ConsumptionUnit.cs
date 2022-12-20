using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum ConsumptionUnit {
        [Display(ShortName = "kg", Name = "kilogram")]
        kg = 0,
        [Display(ShortName = "g", Name = "gram")]
        g = -3
    }
}
