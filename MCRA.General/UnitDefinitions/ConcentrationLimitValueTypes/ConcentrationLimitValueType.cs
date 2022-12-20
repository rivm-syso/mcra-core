
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum ConcentrationLimitValueType {
        Undefined = -1,
        [Display(ShortName = "MRL", Name = "Maximum residue limit")]
        MaximumResidueLimit = 0,
        [Display(ShortName = "Proposed-MRL", Name = "Proposed maximum residue limit")]
        ProposedMaximumResidueLimit = 1,
    }
}
