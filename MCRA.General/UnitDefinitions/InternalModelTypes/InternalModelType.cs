using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum InternalModelType {
        [Description("Use absorption factor model.")]
        [Display(Name = "Absorption Factor Model", ShortName = "AbsorptionFactorModel")]
        AbsorptionFactorModel = 0,
        [Description("Use PBK model.")]
        [Display(Name = "PBK Model", ShortName = "PBKModel")]
        PBKModel = 1,
    }
}
