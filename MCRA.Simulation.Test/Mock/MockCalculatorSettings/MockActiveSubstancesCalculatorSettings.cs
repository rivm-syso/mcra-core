using MCRA.General;
using MCRA.Simulation.Calculators.ActiveSubstancesCalculators.MembershipsFromInSilicoCalculation;

namespace MCRA.Simulation.Test.Mock.MockCalculatorSettings {
    public class MockActiveSubstancesCalculatorSettings : IMembershipsFromInSilicoCalculatorSettings {
        public AssessmentGroupMembershipCalculationMethod AssessmentGroupMembershipCalculationMethod { get; set; }
        public bool IncludeSubstancesWithUnknowMemberships { get; set; }
        public double PriorMembershipProbability { get; set; }
        public bool IsProbabilistic { get; set; }
        public bool UseQsarModels { get; set; }
        public bool UseMolecularDockingModels { get; set; }
        public CombinationMethodMembershipInfoAndPodPresence CombinationMethodMembershipInfoAndPodPresence { get; set; }
        public bool RestrictToAvailableHazardDoses { get; set; }
        public bool RestrictToAvailableRpfs { get; set; }
        public bool UseProbabilisticMemberships { get; set; }
        public bool BubbleMembershipsThroughAop { get; set; }
    }
}
