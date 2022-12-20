using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryDayDrillDownRecord {
        public string Day { get; set; }
        public List<DietaryIntakeSummaryPerFoodRecord> IntakeSummaryPerFoodAsMeasuredRecords { get; set; }
        public List<DietaryIntakeSummaryPerFoodRecord> IntakeSummaryPerFoodAsEatenRecords { get; set; }
        public List<DietaryChronicIntakePerFoodRecord> ChronicIntakePerFoodRecords { get; set; }
        public List<DietaryIntakeSummaryPerCompoundRecord> DietaryIntakeSummaryPerCompoundRecords { get; set; }
        public List<DietaryOthersChronicIntakePerFoodRecord> OthersChronicIntakePerFoodRecords { get; set; }
        public double OthersDietaryIntakePerBodyWeight { get; set; }
        public double TotalDietaryIntakePerBodyWeight { get; set; }
    }
}
