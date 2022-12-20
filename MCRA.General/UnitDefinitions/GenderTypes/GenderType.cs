using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum GenderType {
        [Description("Undefined")]
        [Display(Name = "Undefined", ShortName = "Undefined")]
        Undefined = -1,
        [Description("Female")]
        [Display(Name = "Female", ShortName = "F")]
        Female = 1,
        [Description("Male")]
        [Display(Name = "Male", ShortName = "M")]
        Male = 2,
        
    }
}
