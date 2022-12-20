using MCRA.General;

namespace MCRA.Simulation.Calculators.ActiveSubstancesCalculators.AggregateMembershipModelCalculation {
    public interface IAggregateMembershipModelCalculatorSettings {
        bool UseProbabilisticMemberships { get; }
        bool IncludeSubstancesWithUnknowMemberships { get; }
        bool BubbleMembershipsThroughAop { get; }
        double PriorMembershipProbability { get; }
        AssessmentGroupMembershipCalculationMethod AssessmentGroupMembershipCalculationMethod { get; }
        CombinationMethodMembershipInfoAndPodPresence CombinationMethodMembershipInfoAndPodPresence { get; }
    }
}
