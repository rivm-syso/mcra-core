using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.General {
    public enum AssessmentGroupMembershipCalculationMethod {
        [Display(Name = "Any (crisp)", Order = 1)]
        [Description("Assign the highest membership value as membership. For crisp memberships, assign positive substance membership if any model indicates positive membership, and negative membership otherwise.")]
        CrispMax = 0,
        [Display(Name = "Majority (crisp)", Order = 2)]
        [Description("Assign positive substance membership if the majority of the membership models indicates positive membership, otherwise, the substance is considered not to be in the assessment group.")]
        CrispMajority = 1,
        [Display(Name = "Ratio (probabilistic)", Order = 3)]
        [Description("Express substance membership as a probability ranging from zero (certainly out) to one (certainly in), computed as the average membership score.")]
        ProbabilisticRatio = 2,
        [Display(Name = "Bayesian (probabilistic)", Order = 4)]
        [Description("Express substance memberships as a probability with values ranging from zero (certainly out) to one (certainly in) computed using a Bayesian approach.")]
        ProbabilisticBayesian = 3,
    }
}
