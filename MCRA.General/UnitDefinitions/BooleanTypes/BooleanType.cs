using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum BooleanType {
        [Description("Undefined")]
        [Display(Name = "Undefined", ShortName = "Undefined")]
        Undefined = -1,
        [Description("True")]
        [Display(Name = "True", ShortName = "True")]
        True = 1,
        [Description("False")]
        [Display(Name = "False", ShortName = "False")]
        False = 2
    }
}
