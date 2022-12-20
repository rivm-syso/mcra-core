using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class NonDietaryDrillDownRecord {
        public string Guid { get; set; }
        public string IndividualCode { get; set; }
        public string Day { get; set; }
        public double BodyWeight { get; set; }
        public double SamplingWeight { get; set; }
        public double NonDietaryIntakePerBodyWeight { get; set; }
        public List<RouteIntakeRecord> CorrectedRouteIntakeRecords { get; set; }
        public List<NonDietaryIntakeSummaryPerCompoundRecord> NonDietaryIntakeSummaryPerCompoundRecords { get; set; }
    }
}
