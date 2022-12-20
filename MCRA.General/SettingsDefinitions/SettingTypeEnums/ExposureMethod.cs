using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum ExposureMethod {
        [Description("Exposure levels are determined by explicit specification.")]
        [Display(Name = "Manual")]
        Manual,
        [Description("Exposure levels are generated automatically based on the estimated exposure distribution.")]
        [Display(Name = "Automatic")]
        Automatic,
    }
}
