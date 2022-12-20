
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum ConcentrationValueType {
        [Display(ShortName = "-", Name = "Undefined")]
        Undefined = -1,
        [Display(ShortName = "MC", Name = "Mean concentration")]
        MeanConcentration = 0,
        [Display(ShortName = "MR", Name = "Median concentration")]
        MedianConcentration = 1,
        [Display(ShortName = "HR", Name = "Highest concentration")]
        HighestConcentration = 2,
        [Display(ShortName = "CP", Name = "Concentration percentile")]
        Percentile = 3,
        [Display(ShortName = "LOQ", Name = "Limit of quantification")]
        LOQ = 4,
        [Display(ShortName = "MRL", Name = "Maximum residue limit")]
        MRL = 5,
    }
}
