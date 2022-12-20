using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum ResponseType {
        [Display(ShortName = "CM", Name = "Continuous multiplicative")]
        ContinuousMultiplicative = 0,
        [Display(ShortName = "CA", Name = "Continuous additive")]
        ContinuousAdditive = 1,
        [Display(ShortName = "B", Name = "Binary")]
        Binary = 2,
        [Display(ShortName = "Q", Name = "Quantal")]
        Quantal = 3,
        [Display(ShortName = "QG", Name = "Quantal group")]
        QuantalGroup = 4,
        [Display(ShortName = "C", Name = "Count")]
        Count = 5,
        [Display(ShortName = "O", Name = "Ordinal")]
        Ordinal = 6
    }
}
