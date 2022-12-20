using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum UnitVariabilityCorrelationType {
        [Display(Name = "No correlation")]
        [Description("The unit residue values for unit portions (consumption amount/unitweight) are randomly drawn, explicitly ignoring any correlation between unit residues.")]
        NoCorrelation,
        [Display(Name = "Full correlation")]
        [Description("The unit residue values for unit portions (consumption amount/unitweight) are randomly drawn, explicitly introducing correlation between unit residues, e.g. high (small) values occur more frequently together.")]
        FullCorrelation,
    }
}
