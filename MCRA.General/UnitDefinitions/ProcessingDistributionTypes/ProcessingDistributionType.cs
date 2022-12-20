using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum ProcessingDistributionType {
        [Display(Name = "Logistic Normal distribution", ShortName = "LogisticNormal")]
        LogisticNormal = 1,
        [Display(Name = "Log Normal distribution", ShortName = "LogNormal")]
        LogNormal = 2,
    }
}
