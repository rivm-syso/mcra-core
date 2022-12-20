
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum ConsumptionValueType {
        Undefined = -1,
        [Display(ShortName = "LP", Name = "Large portion")]
        LargePortion = 0,
        [Display(ShortName = "MC", Name = "Mean consumption")]
        MeanConsumption = 1,
        [Display(ShortName = "Percentile", Name = "Percentile")]
        Percentile = 2
    }
}
