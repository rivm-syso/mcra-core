using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum EstimatesNature {
        [Display(Name = "Realistic")]
        [Description("For lognormal: no censoring at the value of the composite sample concentration, no upper limit to the unit concentration. For Beta: no censoring at the value of the composite sample concentration, unit values are never higher than the number of units in composite sample * value of composite sample concentration.")]
        Realistic,
        [Display(Name = "Conservative")]
        [Description("For lognormal: unit values will be left-censored at the value of the composite sample concentration, no upper limit to the unit concentration. For Beta: unit values will be left-censored at the value of the value of composite sample concentration, unit are values never higher than the number of units in composite sample * value of composite sample concentration.")]
        Conservative,
    }
}
