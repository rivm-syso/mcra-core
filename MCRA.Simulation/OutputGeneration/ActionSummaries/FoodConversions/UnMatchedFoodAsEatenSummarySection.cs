using MCRA.Data.Compiled.Wrappers;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UnMatchedFoodAsEatenSummarySection : SummarySection {

        /// <summary>
        /// Summary of conversion results
        /// </summary>
        public List<UnMatchedFoodAsEatenSummaryRecord> Records = new List<UnMatchedFoodAsEatenSummaryRecord>();

        /// <summary>
        /// The number of foods for which no conversion path was found.
        /// </summary>
        public int FoodsNotFound { get; set; }

        public bool SubstanceIndependent { get; set; }

        public void Summarize(ICollection<FoodConversionResult> failedFoodConversionResults, bool substanceIndependent) {
            Records = failedFoodConversionResults
                .GroupBy(fcr => fcr.FoodAsEaten)
                .Select(r => new UnMatchedFoodAsEatenSummaryRecord() {
                    FoodAsEatenCode = r.Key.Code,
                    FoodAsEatenName = r.Key.Name,
                })
                .OrderBy(r => r.FoodAsEatenName, StringComparer.OrdinalIgnoreCase)
                .ToList();
            Records.TrimExcess();
            FoodsNotFound = Records.Count;
            SubstanceIndependent = substanceIndependent;
        }
    }
}
