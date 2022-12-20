using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    /// <summary>
    /// Only the polynomial is implemented
    /// </summary>
    public enum FunctionType {
        [Display(Name = "Polynomial")]
        [Description("A polynomial regression fits a nonlinear relationship between the value of the independent variable (e.g. age) and the corresponding conditional mean of y (here the exposure). A polynomial with a degree of 0 is simply a constant function; with a degree of 1 is a line; with a degree of 2 is a quadratic; with a degree of 3 is a cubic, and so on.")]
        Polynomial,
        [Display(Name = "Spline")]
        [Description("A spline fits a nonlinear relationship between the value of the independent variable (e.g. age) and the corresponding conditional mean of y (here the exposure). A spline with a degree of 0 is simply a constant function; with a degree of 1 is a line; with a degree of 2 is a quadratic; with a degree of 3 is a cubic, and so on.")]
        Spline,
    }
}
