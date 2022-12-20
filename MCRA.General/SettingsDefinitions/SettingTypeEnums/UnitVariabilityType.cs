using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum UnitVariabilityType {
        [Display(Name = "Variation coefficient")]
        [Description("Standard deviation divided by the mean.")]
        VariationCoefficient,
        [Display(Name = "Variability factor")]
        [Description("Defined as 97.5th percentile divided by the mean.")]
        VariabilityFactor,
    }
}
