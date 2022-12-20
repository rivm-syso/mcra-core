using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum UnitWeightValueType {
        [Display(Name = "Unit weight raw agricultural commodity", ShortName = "RAC")]
        UnitWeightRac = 0,
        [Display(Name = "Unit Weight edible portion", ShortName = "EP")]
        UnitWeightEp = 1,
    }
}
