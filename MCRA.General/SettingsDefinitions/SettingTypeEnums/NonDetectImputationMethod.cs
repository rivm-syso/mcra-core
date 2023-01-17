using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum NonDetectImputationMethod {
        [Display(Name = "Replace by LOR/LOQ/LOD", ShortName = "ReplaceLimit")]
        [Description("Non-quantifications are replaced by f * LOR or f * LOD or by LOD + f * (LOQ - LOD) where f is a constant.")]
        ReplaceByLimit = 0,
        [Display(Name = "Impute from censored lognormal distribution", ShortName = "Impute from censoredln")]
        [Description("Replace nondetect measurements by a random draw from the lower (left) tail of the censored lognormal distribution.")]
        CensoredLogNormal = 1
    }
}
