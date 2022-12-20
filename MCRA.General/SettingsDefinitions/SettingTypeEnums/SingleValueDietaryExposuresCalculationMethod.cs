using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    /// <summary>
    /// Use of EFSA Pesticide Residue Intake Model (EFSA PRIMo revision 3), ADOPTED: 19 December 2017
    /// Chronic. doi: 10.2903/j.efsa.2018.5147
    /// </summary>
    public enum SingleValueDietaryExposuresCalculationMethod {
        [Display(Name = "IESTI", Order = 1)]
        [Description("IESTI.")]
        IESTI,
        [Display(Name = "IESTI new", Order = 2)]
        [Description("IESTI new.")]
        IESTINew,
        [Description("Theoretical Maximum Daily Intake.")]
        [Display(Name = "TMDI", ShortName = "TMDI", Order = 3)]
        TMDI,
        [Description("International Estimated Daily Intake.")]
        [Display(Name = "IEDI", ShortName = "IEDI", Order = 4)]
        IEDI,
        [Description("Rees–Day model (I).")]
        [Display(Name = "Rees–Day model (I)", ShortName = "Rees–Day (I)", Order = 5)]
        NEDI1,
        [Description("Rees–Day model (II).")]
        [Display(Name = "Rees–Day model (II)", ShortName = "Rees–Day (II)", Order = 6)]
        NEDI2,
    }
}
