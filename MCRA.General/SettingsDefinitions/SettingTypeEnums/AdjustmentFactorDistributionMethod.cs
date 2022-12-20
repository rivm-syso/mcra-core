using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    /// <summary>
    /// https://stats.stackexchange.com/questions/325154/log-student-t-distribution-calculated-mean
    ///
    /// </summary>
    public enum AdjustmentFactorDistributionMethod {
        [Description("No adjustment factor")]
        [Display(Name = "No adjustment factor", ShortName = "None")]
        None = 0,
        [Description("Fixed adjustment factor")]
        [Display(Name = "Fixed adjustment factor", ShortName = "Fixed")]
        Fixed = 1,
        [Description("Lognormal distribution with parameters a and b and offset c (default c = 0)")]
        [Display(Name = "Lognormal", ShortName = "Lognormal")]
        LogNormal = 2,
        [Description("Log Students-t distribution with parameters a, b and c and offset d (default d = 0)")]
        [Display(Name = "Log Students-t", ShortName = "Log Students-t")]
        LogStudents_t = 3,
        [Description("Beta distribution with shape parameters a and b on interval [c, d], (default = 0, 1)")]
        [Display(Name = "Beta", ShortName = "Beta")]
        Beta = 4,
        [Description("Gamma distribution with shape parameter a and rate parameter b with offset = c (default = 0)")]
        [Display(Name = "Gamma", ShortName = "Gamma")]
        Gamma = 5,
    }
}
