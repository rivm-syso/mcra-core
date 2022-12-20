using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum IndividualSubsetType {
        [Description("Match individuals selection to population definition.")]
        [Display(Name = "Match individuals selection to population definition", ShortName = "Match to population definition")]
        MatchToPopulationDefinition = 1,
        [Description("Ignore population definition (use all individuals in survey).")]
        [Display(Name = "Ignore population definition (use all individuals in survey)", ShortName = "Ignore population definition")]
        IgnorePopulationDefinition = 2,
        [Description("Match individuals selection to population definition using selected properties only.")]
        [Display(Name = "Match individuals selection to population definition using selected properties only", ShortName = "Match using selected properties")]
        MatchToPopulationDefinitionUsingSelectedProperties = 3,
    }
}
