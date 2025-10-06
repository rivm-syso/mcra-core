using MCRA.Simulation.Calculators.ModelledFoodsCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelledFoodsSummarySection : SummarySection {

        public List<ModelledFoodSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<ModelledFoodInfo> modelledFoodsInfos) {
            Records = modelledFoodsInfos
                .GroupBy(r => r.Food)
                .Select(c => {
                    return new ModelledFoodSummaryRecord() {
                        FoodCode = c.Key.Code,
                        FoodName = c.Key.Name
                    };
                })
                .OrderBy(c => c.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
