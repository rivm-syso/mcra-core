using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum PointOfDepartureType {
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
        [Description("NOEL")]
        [Display(Name = "No observed effect level", ShortName = "NOEL")]
        Noel = 4,
        [Description("LD50")]
        [Display(Name = "Median lethal dose", ShortName = "LD50")]
        Ld50 = 5,
        [Description("BMDL01")]
        [Display(Name = "Benchmark dose lower confidence limit of 1%", ShortName = "BMDL01")]
        Bmdl01 = 6,
        [Description("BMDL10")]
        [Display(Name = "Benchmark dose lower confidence limit of 10%", ShortName = "BMDL10")]
        Bmdl10 = 7,
    }
}
