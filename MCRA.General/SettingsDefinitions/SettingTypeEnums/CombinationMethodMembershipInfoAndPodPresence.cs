using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum CombinationMethodMembershipInfoAndPodPresence {
        [Display(Name = "Consider active if POD/HC AND (in-silico) memberships indicate active", Order = 2)]
        Intersection = 0,
        [Display(Name = "Consider active if POD/HC OR (in-silico) memberships indicates active", Order = 1)]
        Union = 1
    }
}
