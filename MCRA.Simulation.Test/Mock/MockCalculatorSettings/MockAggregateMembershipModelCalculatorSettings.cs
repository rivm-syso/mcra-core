using MCRA.General;
using MCRA.Simulation.Calculators.ActiveSubstancesCalculators.AggregateMembershipModelCalculation;

namespace MCRA.Simulation.Test.Mock.MockCalculatorSettings {
    public class MockAggregateMembershipModelCalculatorSettings : IAggregateMembershipModelCalculatorSettings {
        public bool UseProbabilisticMemberships { get; set; }
        public bool BubbleMembershipsThroughAop { get; set; }
        public bool IncludeSubstancesWithUnknowMemberships { get; set; }
        public double PriorMembershipProbability { get; set; }
        public AssessmentGroupMembershipCalculationMethod AssessmentGroupMembershipCalculationMethod { get; set; }
        public CombinationMethodMembershipInfoAndPodPresence CombinationMethodMembershipInfoAndPodPresence { get; set; }
    }
}
