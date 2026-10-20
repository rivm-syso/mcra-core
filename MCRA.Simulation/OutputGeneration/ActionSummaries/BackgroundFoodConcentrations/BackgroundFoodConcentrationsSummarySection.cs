using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class BackgroundFoodConcentrationsSummarySection : SummarySection {

        public List<BackgroundFoodConcentrationRecord> Records { get; set; }

        public void Summarize(ICollection<BackgroundFoodConcentration> backgroundFoodConcentrations) {
            Records = backgroundFoodConcentrations
                .Select(r => new BackgroundFoodConcentrationRecord() {
                    FoodName = r.Food.Name,
                    FoodCode = r.Food.Code,
                    SubstanceName = r.Substance?.Name,
                    SubstanceCode = r.Substance?.Code,
                    Value = r.Value,
                    Unit = r.ConcentrationUnit.GetShortDisplayName(),
                    Reference = r.Reference,
                })
                .OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
