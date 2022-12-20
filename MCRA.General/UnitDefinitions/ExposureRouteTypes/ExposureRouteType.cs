using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum ExposureRouteType {
        Undefined = -1,
        [Display(Name = "Dietary exposure", ShortName = "Dietary")]
        Dietary,
        [Display(Name = "Non-dietary oral exposure", ShortName = "Oral")]
        Oral,
        [Display(Name = "Non-dietary dermal exposure", ShortName = "Dermal")]
        Dermal,
        [Display(Name = "Non-dietary inhalation exposure", ShortName = "Inhalation")]
        Inhalation,
        [Display(Name = "At target", ShortName = "At target")]
        AtTarget,
    }
}
