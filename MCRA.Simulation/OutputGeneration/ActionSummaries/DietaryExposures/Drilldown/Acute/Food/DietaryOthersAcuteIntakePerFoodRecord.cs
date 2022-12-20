using System.Collections.Generic;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Acute summary record for a food (as measured, as eaten)
    /// </summary>
    public sealed class DietaryOthersAcuteIntakePerFoodRecord {
        public string FoodAsMeasuredName { get; set; }
        public List<DietaryOthersAcuteIntakePerCompoundRecord> OthersAcuteIntakePerCompoundRecords { get; set; }
    }
}
