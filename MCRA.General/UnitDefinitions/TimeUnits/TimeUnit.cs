using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum TimeUnit {
        [Description("In hours")]
        [Display(Name = "hours", ShortName = "h")]
        Hours = 0,
        [Description("In minutes")]
        [Display(Name = "minutes", ShortName = "min")]
        Minutes = 1,
        [Description("Unknown")]
        [Display(Name = "unknown", ShortName = "unknown")]
        NotSpecified = 1,
    }
}
