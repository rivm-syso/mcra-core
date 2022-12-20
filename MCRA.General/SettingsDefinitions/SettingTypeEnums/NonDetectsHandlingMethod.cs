using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum NonDetectsHandlingMethod {
        [Display(Name = "By zero")]
        [Description("Non-quantifications are assumed to be zero's (set to 0).")]
        ReplaceByZero,
        [Display(Name = "By f * LOR")]
        [Description("Non-quantifications are replaced by f * LOR where f is a constant.")]
        ReplaceByLOR,
        [Display(Name = "By f * LOD or by LOD + f * (LOQ - LOD)")]
        [Description("Left censored are replaced by f * LOD; Non-quantifications are replaced by LOD + f * (LOQ - LOD), where f is a constant.")]
        ReplaceByLODLOQSystem,
    }
}
