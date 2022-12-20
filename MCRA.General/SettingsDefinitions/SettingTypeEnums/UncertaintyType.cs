using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    /// <summary>
    /// Obsolete
    /// </summary>
    public enum UncertaintyType {
        [Display(Name = "Empirical")]
        [Description("Data are taken as such.")]
        Empirical,
        [Display(Name = "Parametric")]
        [Description("A parametric model is fitted to the data.")]
        Parametric,
    }
}
