using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum DietaryExposuresDetailsLevel {
        [Display(Name = "Full")]
        [Description("Show all details.")]
        Full = 0,
        [Display(Name = "Restrict to risk-drivers (dietary exposures screening)")]
        [Description("Restrict to detailed output for risk-drivers identified by dietary exposures screening.")]
        OnlyRiskDrivers = 1,
        [Display(Name = "Omit foods-as-eaten details")]
        [Description("Restrict to detailed output for modelled foods and substances. Omit foods-as-eaten details.")]
        OmitFoodsAsEaten = 2
    }
}
