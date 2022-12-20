using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum HazardDoseImputationMethodType {
        [Display(Name = "Munro P5 (TTC approach)")]
        [Description("Use the P5 of the Munro NOEL collection")]
        MunroP5,
        [Display(Name = "Munro central value")]
        [Description("Use an unbiased nominal value from the Munro NOEL collection; draw randomly from this collection in the uncertainty runs")]
        MunroUnbiased,
        [Display(Name = "Available hazard characterisations distribution P5")]
        [Description("Use the P5 of the available points of departure")]
        HazardDosesP5,
        [Display(Name = "Available hazard characterisations distribution central value")]
        [Description("Use an unbiased nominal value from the collection of available points of departure; draw randomly from this collection in the uncertainty runs")]
        HazardDosesUnbiased,
    }
}
