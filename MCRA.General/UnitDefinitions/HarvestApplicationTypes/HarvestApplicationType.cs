using System.ComponentModel.DataAnnotations;

namespace MCRA.General {

    public enum HarvestApplicationType {
        Undefined = -1,
        [Display(ShortName = "Pre-harvest", Name = "Pre-harvest application")]
        PreHarvest,
        [Display(ShortName = "Post-harvest", Name = "Post-harvest application")]
        PostHarvest
    }
}
