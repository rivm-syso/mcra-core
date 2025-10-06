using MCRA.Simulation.Calculators.ModelledFoodsCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelledFoodSubstancesSummarySection : SummarySection {

        public List<ModelledFoodSubstanceSummaryRecord> Records { get; set; }

        public void Summarize(ICollection<ModelledFoodInfo> modelledFoodsInfos) {
            Records = modelledFoodsInfos
                .Select(c => {
                    return new ModelledFoodSubstanceSummaryRecord() {
                        FoodCode = c.Food.Code,
                        FoodName = c.Food.Name,
                        SubstanceCode = c.Substance.Code,
                        SubstanceName = c.Substance.Name,
                        HasMrl = c.HasMrl,
                        HasMeasurements = c.HasMeasurements,
                        HasPositiveConcentrations = c.HasPositiveMeasurements
                    };
                })
                .OrderBy(c => c.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.SubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.SubstanceCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
