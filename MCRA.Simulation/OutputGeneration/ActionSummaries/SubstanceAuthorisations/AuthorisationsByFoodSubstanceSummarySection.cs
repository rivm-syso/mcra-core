using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class AuthorisationsByFoodSubstanceSummarySection : SummarySection {

        public List<AuthorisationByFoodSubstanceSummaryRecord> Records { get; set; }

        public void Summarize(IDictionary<(Food, Compound), SubstanceAuthorisation> substanceAuthorisations) {
            Records = substanceAuthorisations.Values
                .Select(r => new AuthorisationByFoodSubstanceSummaryRecord() {
                        FoodCode = r.Food.Code,
                        FoodName = r.Food.Name,
                        SubstanceCode = r.Substance.Code,
                        SubstanceName = r.Substance.Name,
                        Reference = r.Reference,
                    }
                )
                .OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.SubstanceCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
