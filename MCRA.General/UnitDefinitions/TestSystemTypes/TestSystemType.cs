using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum TestSystemType {
        Undefined = -1,
        [Display(ShortName = "In vivo", Name = "In vivo")]
        InVivo,
        [Display(ShortName = "Cell line", Name = "Cell line")]
        CellLine,
        [Display(ShortName = "Primary cells", Name = "Primary cells")]
        PrimaryCells,
        [Display(ShortName = "Tissue", Name = "Tissue")]
        Tissue,
        [Display(ShortName = "Organ", Name = "Organ")]
        Organ
    }
}
