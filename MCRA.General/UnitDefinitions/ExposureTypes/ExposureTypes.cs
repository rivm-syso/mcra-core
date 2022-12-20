using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum ExposureType {
        [Description("Acute, short term.")]
        [Display(Name = "Acute")]
        Acute,
        [Description("Chronic, long term.")]
        [Display(Name = "Chronic")]
        Chronic,
    }
}
