using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum InternalConcentrationType {
        [Display(Name = "Internal modelled concentrations")]
        [Description("Internal modelled concentrations from dietary and/or non-dietary routes, aggregated.")]
        ModelledConcentration,
        [Display(Name = "Human monitoring concentrations")]
        [Description("Human monitoring concentrations as measured in blood, urine or other compartments.")]
        MonitoringConcentration
    }
}
