using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum SurfaceAreaUnit {
        [Display(ShortName = "cm2", Name = "square centimeters")]
        SquareCentimeters,
        [Display(ShortName = "dm2", Name = "square decimeters")]
        SquareDecimeters,
        [Display(ShortName = "m2", Name = "square meters")]
        SquareMeters,
    };
}
