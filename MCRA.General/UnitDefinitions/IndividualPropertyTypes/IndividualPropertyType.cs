using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum IndividualPropertyType {
        [Description("Categorical")]
        [Display(Name = "Categorical", ShortName = "Categorical")]
        Categorical = 0,
        [Description("Boolean")]
        [Display(Name = "Boolean", ShortName = "Boolean")]
        Boolean = 1,
        [Description("Numeric")]
        [Display(Name = "Numeric", ShortName = "Numeric")]
        Numeric = 2,
        [Description("Nonnegative")]
        [Display(Name = "Nonnegative", ShortName = "Nonnegative")]
        Nonnegative = 3,
        [Description("Integer")]
        [Display(Name = "Integer", ShortName = "Integer")]
        Integer = 4,
        [Description("NonnegativeInteger")]
        [Display(Name = "NonnegativeInteger", ShortName = "NonnegativeInteger")]
        NonnegativeInteger = 5,
        [Description("Month")]
        [Display(Name = "Month", ShortName = "Month")]
        Month = 6,
        [Description("DateTime")]
        [Display(Name = "DateTime", ShortName = "DateTime")]
        DateTime = 7,
        [Description("Gender")]
        [Display(Name = "Gender", ShortName = "Gender")]
        Gender = 8,
        [Description("Location")]
        [Display(Name = "Location", ShortName = "Location")]
        Location = 9
    }
}
