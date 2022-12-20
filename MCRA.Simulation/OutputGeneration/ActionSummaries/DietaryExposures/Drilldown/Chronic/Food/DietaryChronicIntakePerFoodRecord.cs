using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DietaryChronicIntakePerFoodRecord {
        public string FoodAsMeasuredCode { get; set; }
        public string FoodAsMeasuredName { get; set; }
        public double FoodAsMeasuredAmount { get; set; }
        public string FoodAsEatenName { get; set; }
        public string FoodAsEatenCode { get; set; }
        public double FoodAsEatenAmount { get; set; }
        public double Translation { get; set; }
        public List<DietaryChronicIntakePerCompoundRecord> ChronicIntakePerCompoundRecords { get; set; }
    }
}
