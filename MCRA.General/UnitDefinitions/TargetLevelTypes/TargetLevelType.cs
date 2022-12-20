using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum TargetLevelType {
        [Display(Name = "External", ShortName = "Ext")]
        [Description("Hazard characterisations are derived for external exposures")]
        External = 0,
        [Display(Name = "Internal", ShortName = "Int")]
        [Description("Hazard characterisations are derived for internal exposures")]
        Internal = 1,
    }
}
