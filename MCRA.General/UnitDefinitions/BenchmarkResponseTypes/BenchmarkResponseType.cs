using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum BenchmarkResponseType {
        [Display(ShortName = "Undefined", Name = "Undefined")]
        Undefined = -1,
        [Display(ShortName = "Fraction change", Name = "Fraction change")]
        FractionChange = 7,
        [Display(ShortName = "Fraction of background", Name = "Fraction of background response")]
        Factor = 0,
        [Display(ShortName = "Percentage of background", Name = "Percentage of background response")]
        Percentage = 1,
        [Display(ShortName = "Percentage change", Name = "Percentage change")]
        PercentageChange = 8,
        [Display(ShortName = "Threshold value", Name = "Absolute threshold value")]
        Absolute = 2,
        [Display(ShortName = "Absolute difference", Name = "Absolute difference")]
        Difference = 3,
        [Display(ShortName = "ER", Name = "Extra risk")]
        ExtraRisk = 4,
        [Display(ShortName = "AR", Name = "Additional risk")]
        AdditionalRisk = 5,
        [Display(ShortName = "ED50", Name = "ED50")]
        Ed50 = 6,
    }
}
