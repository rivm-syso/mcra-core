using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum TimeScaleUnit {
        Unspecified = -1,
        [Display(ShortName = "day", Name = "day")]
        PerDay,
        [Display(ShortName = "steady state", Name = "steady state")]
        SteadyState,
        [Display(ShortName = "peak", Name = "peak")]
        Peak
    }
}
