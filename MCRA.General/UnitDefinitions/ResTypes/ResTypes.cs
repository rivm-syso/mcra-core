using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum ResType {
        [Description("VAL.")]
        [Display(Name = "VAL")]
        VAL,
        [Description("LOD.")]
        [Display(Name = "LOD")]
        LOD,
        [Description("LOQ.")]
        [Display(Name = "LOQ")]
        LOQ,
        [Description("MV.")]
        [Display(Name = "MV")]
        MV,
    }
}
