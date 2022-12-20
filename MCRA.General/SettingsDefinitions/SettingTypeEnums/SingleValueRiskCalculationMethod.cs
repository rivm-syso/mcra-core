using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum SingleValueRiskCalculationMethod {
        [Description("From single value dietary exposures and hazard characterisations.")]
        [Display(Name = "From single value dietary exposures", ShortName = "From single value dietary exposures")]
        FromSingleValues = 0,
        [Description("As percentile from risks distribution.")]
        [Display(Name = "As percentile from risks distribution", ShortName = "As percentile")]
        FromIndividualRisks = 1,
    }
}
