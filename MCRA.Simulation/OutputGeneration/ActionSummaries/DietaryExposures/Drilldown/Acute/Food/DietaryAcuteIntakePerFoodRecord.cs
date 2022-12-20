using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Acute summary record for a food (as measured, as eaten)
    /// </summary>
    public sealed class DietaryAcuteIntakePerFoodRecord {
        public string FoodAsMeasuredCode { get; set; }
        public string FoodAsMeasuredName { get; set; }
        public string FoodAsEatenName { get; set; }
        public string FoodAsEatenCode { get; set; }
        public double FoodAsEatenAmount { get; set; }
        public double Translation { get; set; }
        public List<DietaryAcuteIntakePerCompoundRecord> AcuteIntakePerCompoundRecords { get; set; }
    }
}
