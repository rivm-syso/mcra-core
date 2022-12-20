using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum MultipleSubstanceHandlingMethod {
        [Description("Combined assessment of selected substances.")]
        [Display(Name = "Combined assessment of selected substances", ShortName = "Combined")]
        Cumulate = 0,
        [Description("Loop over selected substances.")]
        [Display(Name = "Loop over selected substances", ShortName = "Loop")]
        LoopSubstances = 1,
    }
}
