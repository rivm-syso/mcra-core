using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum CompoundGroupSelectionMethodType {
        [Display(Name = "All substances", Order = 1)]
        [Description("Include all substances of the substances table and use hazard characterisation imputation for missing hazard data.")]
        IncludeAll = 1,
        [Display(Name = "Restrict to available hazard data", Order = 2)]
        [Description("Restrict to the substances with available hazard data (either in the form of dose response models or RPFs).")]
        RestrictHazardDoseRpf = 0,
        [Display(Name = "Restrict to available hazard data and possible membership", Order = 3)]
        [Description("Consider only the substances with available hazard data and non-zero membership (i.e., P(AG) > 0).")]
        RestrictHazardDoseRpfAndProbableMembership = 3,
        [Display(Name = "Restrict to available hazard data and certain membership", Order = 4)]
        [Description("Consider only substances with certain assessment group membership (i.e., P(AG) = 1) and for which a hazard characterisation is available.")]
        RestrictHazardDoseRpfAndCertainMembership = 5,
        [Display(Name = "Restrict to non-zero membership", Order = 5)]
        [Description("Consider all substances, use TTC based on the Cramer class for the substances for which no limit dose or RPF is defined.")]
        RestrictProbableMembership = 2,
        [Display(Name = "Restrict to certain membership", Order = 6)]
        [Description("Consider only the substances with certain assessment group membership (i.e., P(AG) = 1).")]
        RestrictCertainMembership = 4,
    }
}
