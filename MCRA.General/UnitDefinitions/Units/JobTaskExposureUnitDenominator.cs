using System.ComponentModel.DataAnnotations;

namespace MCRA.General.UnitDefinitions.Units {
    public enum JobTaskExposureUnitDenominator {
        None = 0,
        [Display(Name = "kg")]
        Kilograms,
        [Display(Name = "cm2")]
        SquareCentimeters,
        [Display(Name = "m3")]
        CubicMeters
    }
}
