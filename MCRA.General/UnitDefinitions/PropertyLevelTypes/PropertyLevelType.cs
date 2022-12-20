using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum PropertyLevelType {
        [Description("Individual level")]
        [Display(Name = "Individual", ShortName = "Individual")]
        Individual = 0,
        [Description("Individual day level")]
        [Display(Name = "IndividualDay", ShortName = "IndividualDay")]
        IndividualDay = 1,
    }
}
