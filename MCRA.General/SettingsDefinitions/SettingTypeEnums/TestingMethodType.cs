using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum TestingMethodType {
        [Display(Name = "Backward")]
        [Description("Backward selection starts with selecting a model with a function of the highest degree. Then, the degree of the function is decreased by one and the model is tested again. This process is repeated until decreasing the degree does not improve the model fit anymore.")]
        Backward,
        [Display(Name = "Forward")]
        [Description("Forward selections starts with selecting a model with a function of the lowest degree. Then, the degree of the function is increased by one and the model is tested again. This process is repeated until increasing the degree does not improve the model fit anymore.")]
        Forward,
    }
}
