using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum ConcentrationModelChoice {
        [Display(Name = "Custom", Order = 100)]
        [Description("By setting this tier to custom, the exposure model can be configured in any way desirable (without tier specific presets). The model is fully specified by the user. Both EFSA 2012 Optimistic and EFSA 2012 Pessimistic can be specified using the custom model choice. This choice allows for a sensitivity analysis where each factor is varied, one at the time.")]
        Custom = 0,
        [Display(Name = "EFSA 2012 Optimistic", Order = 1)]
        EfsaOptimistic = 1,
        [Display(Name = "EFSA 2012 Pessimistic", Order = 2)]
        EfsaPessimistic = 2,
        [Display(Name = "EC 2018 Tier 1", Order = 5)]
        ComTier1 = 4,
        [Display(Name = "EC 2018 Tier 2", Order = 6)]
        ComTier2 = 5,
        [Display(Name = "EFSA 2012 Pessimistic - Acute", Order = 3)]
        EfsaPessimisticAcute = 6,
        [Display(Name = "EFSA 2012 Pessimistic - Chronic", Order = 4)]
        EfsaPessimisticChronic = 7,
    }
}
