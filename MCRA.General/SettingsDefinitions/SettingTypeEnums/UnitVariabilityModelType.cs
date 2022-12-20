using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum UnitVariabilityModelType {
        [Display(Name = "Beta distribution")]
        [Description("Requires knowledge of the number of units in a composite sample, and of the variability between units (realistic or conservative estimates). Under the beta model, the simulated unit values are drawn from a bounded distribution on the interval.")]
        BetaDistribution,
        [Display(Name = "Lognormal distribution")]
        [Description("Requires only knowledge of the variability between units (realistic or conservative estimates). The lognormal distribution is considered as an appropriate model for many empirical positive concentration distributions (unbounded distribution).")]
        LogNormalDistribution,
        [Display(Name = "Bernoulli distribution")]
        [Description("Requires only knowledge of the number of units in a composite sample (results are always conservative). The bernoulli model is a limiting case of the beta model, which can be used if no information on unit variability is available, but only the number of units in a composite sample is known.")]
        BernoulliDistribution,
    }
}
