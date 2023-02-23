using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class UnMatchedModelledFoodSummarySection : SummarySection {

        /// <summary>
        /// Summary of conversion results
        /// </summary>
        public List<UnMatchedModelledFoodSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<Food> modelledFoods) {
            Records = modelledFoods
                .Select(c => new UnMatchedModelledFoodSummaryRecord() {
                    FoodAsMeasuredCode = c.Code,
                    FoodAsMeasuredName = c.Name,
                })
                .ToList();
        }
    }
}
