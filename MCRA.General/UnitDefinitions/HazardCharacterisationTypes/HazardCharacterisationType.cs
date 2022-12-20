using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum HazardCharacterisationType {
        [Description("Unspecified")]
        [Display(Name = "Unspecified PoD", ShortName = "Unspecified")]
        Unspecified = 0,
        [Description("BMD")]
        [Display(Name = "Benchmark dose", ShortName = "BMD")]
        Bmd = 1,
        [Description("NOAEL")]
        [Display(Name = "No observed adverse effect level", ShortName = "NOAEL")]
        Noael = 2,
        [Description("LOAEL")]
        [Display(Name = "Lowest observed adverse effect level", ShortName = "LOAEL")]
        Loael = 3,
        [Description("ADI")]
        [Display(Name = "Acceptable daily intake", ShortName = "ADI")]
        Adi = 4,
        [Description("ARfD")]
        [Display(Name = "Acute reference dose", ShortName = "ARfD")]
        Arfd = 5,
        [Description("NOEL")]
        [Display(Name = "No observed effect level", ShortName = "NOEL")]
        Noel = 6,
        [Description("TWI")]
        [Display(Name = "Tolerable weekly intake", ShortName = "TWI")]
        Twi = 7,
        [Description("BMDL01")]
        [Display(Name = "Benchmark dose lower confidence limit of 1%", ShortName = "BMDL01")]
        Bmdl01 = 8,
        [Description("BMDL10")]
        [Display(Name = "Benchmark dose lower confidence limit of 10%", ShortName = "BMDL10")]
        Bmdl10 = 9,
        [Description("TDI")]
        [Display(Name = "Tolerable daily intake", ShortName = "TDI")]
        Tdi = 10,
        [Description("HBM-GV")]
        [Display(Name = "Human biomonitoring guidance values", ShortName = "HBMGV")]
        Hbmgv = 11,
        [Description("Other")]
        [Display(Name = "Other", ShortName = "Other")]
        Other = 12,
    }
}
