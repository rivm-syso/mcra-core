using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum VolumeUnit {
        Undefined = -1,
        [Display(ShortName = "m3", Name = "cubicmeter")]
        Cubicmeter,
        [Display(ShortName = "L", Name = "liter")]
        Liter
    };
}
