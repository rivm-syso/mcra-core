using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum SubstanceTranslationAllocationMethod {
        [Display(Name = "Random allocation")]
        [Description("Random allocation.")]
        DrawRandom = 0,
        [Display(Name = "Allocate most potent")]
        [Description("Allocate most potent active substance.")]
        UseMostToxic = 1,
        [Display(Name = "Nominal estimate")]
        [Description("Allocate nominal estimate (weighted average allocation).")]
        NominalEstimates = 2,
        [Display(Name = "Allocate to all")]
        [Description("Allocate for each active substance independently as if all concentrations were allocated to this active substance.")]
        AllocateToAll = 3,
    };
}
