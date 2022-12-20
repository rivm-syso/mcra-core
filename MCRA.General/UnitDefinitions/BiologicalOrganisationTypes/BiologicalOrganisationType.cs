using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum BiologicalOrganisationType {
        [Description("Unspecified")]
        [Display(Name = "Unspecified", ShortName = "Unspecified")]
        Unspecified = 0,

        [Description("Molecular")]
        [Display(Name = "Molecular", ShortName = "Molecular")]
        Molecular = 1,

        [Description("Cellular")]
        [Display(Name = "Cellular", ShortName = "Cellular")]
        Cellular = 2,

        [Description("Tissue")]
        [Display(Name = "Tissue", ShortName = "Tissue")]
        Tissue = 3,

        [Description("Organ")]
        [Display(Name = "Organ", ShortName = "Organ")]
        Organ = 4,

        [Description("Individual")]
        [Display(Name = "Individual", ShortName = "Individual")]
        Individual = 5,

        [Description("Population")]
        [Display(Name = "Population", ShortName = "Population")]
        Population = 6
    }
}
