using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum PointOfDeparture {
        [Description("Do not convert non-standard point of departures.")]
        [Display(Name = "Unspecified (no conversion to common expression type)")]
        FromReference,
        [Description("Convert all point of departures to bench mark doses.")]
        [Display(Name = "BMD (convert all hazard characterisations as BMDs)")]
        BMD,
        [Description("Convert all point of departures to NOAELs.")]
        [Display(Name = "NOAEL (convert all hazard characterisations as NOAELs)")]
        NOAEL,
    };
}
