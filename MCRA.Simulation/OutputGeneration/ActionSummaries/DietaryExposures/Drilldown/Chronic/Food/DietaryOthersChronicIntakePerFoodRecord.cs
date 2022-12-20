using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryOthersChronicIntakePerFoodRecord {
        public string FoodAsMeasuredName { get; set; }
        public List<DietaryOthersChronicIntakePerCompoundRecord> OthersChronicIntakePerCompoundRecords { get; set; }
    }
}
