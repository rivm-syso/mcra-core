using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryAcuteDrillDownRecord {
        public string Guid { get; set; }
        public string IndividualCode { get; set; }
        public string Day { get; set; }
        public double BodyWeight { get; set; }
        public double SamplingWeight { get; set; }
        public double DietaryIntakePerMassUnit { get; set; }
        public double OthersIntakePerMassUnit { get; set; }
        public double DietaryAbsorptionFactor { get; set; }
        public List<DietaryIntakeSummaryPerFoodRecord> IntakeSummaryPerFoodAsMeasuredRecords { get; set; }
        public List<DietaryIntakeSummaryPerFoodRecord> IntakeSummaryPerFoodAsEatenRecords { get; set; }
        public List<DietaryAcuteIntakePerFoodRecord> AcuteIntakePerFoodRecords { get; set; }
        public List<DietaryIntakeSummaryPerCompoundRecord> IntakeSummaryPerCompoundRecords { get; set; }
        public List<DietaryOthersAcuteIntakePerFoodRecord> OthersAcuteIntakePerFoodRecords { get; set; }
    }
}
