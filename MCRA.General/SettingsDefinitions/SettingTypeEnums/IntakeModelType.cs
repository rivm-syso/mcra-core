using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum IntakeModelType {
        [Display(Name = "Observed Individual Means", ShortName = "OIM", Order = 1)]
        [Description("Observed Individual Means: just the empirical means over the observed days.")]
        OIM,
        [Display(Name = "BetaBinomial Normal", ShortName = "BBN", Order = 2)]
        [Description("BetaBinomial distribution for frequency of exposure + (transformed) Normal distribution for amounts (de Boer et al. 2009).")]
        BBN,
        [Display(Name = "Logistic-Normal Normal", ShortName = "LNN0", Order = 3)]
        [Description("Logistic-Normal distribution for frequency of exposure + (transformed) Normal distribution for amounts.")]
        LNN0,
        [Display(Name = "Logistic-Normal Normal with correlation", ShortName = "LNN", Order = 4)]
        [Description("Logistic-Normal distribution for frequency of exposure + (transformed) Normal distribution for amounts. Both models are estimated taking into account the correlation between exposure frequency and amounts.")]
        LNN,
        [Display(Name = "Iowa State University Foods model", ShortName = "ISUF", Order = 5)]
        [Description("Iowa State University Foods model: semiparametric distribution for frequency of exposure + (transformed) Normal distribution for amounts (de Boer et al. 2009, Dodd (1996)).")]
        ISUF,
    }
}
