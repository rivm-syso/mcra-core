using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum RiskMetricType {
        [Display(Name = "Margin of exposure", ShortName = "MOE")]
        [Description("Margin of exposure (MOE).")]
        MarginOfExposure,
        [Display(Name = "Hazard index", ShortName = "HI")]
        [Description("Hazard index (HI).")]
        HazardIndex
    }
}
