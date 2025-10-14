using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.SubstanceAuthorisationsBuilder {
    public class SubstanceAuthorisationsBuilder {

        public static List<SubstanceAuthorisation> ComputeFocalSubstanceAuthorisations(
            ICollection<(Food Food, Compound Substance)> authorisations,
            ICollection<(Food Food, Compound Substance)> focalCommodityCombinations,
            ICollection<SubstanceConversion> substanceConversions
        ) {
            var result = focalCommodityCombinations
                .Where(c => !authorisations.Contains((c.Food, c.Substance)))
                .ToHashSet();

            if (substanceConversions?.Count > 0) {
                foreach (var combination in focalCommodityCombinations) {
                    var conversions = substanceConversions
                        .Where(c => c.MeasuredSubstance == combination.Substance
                            && !authorisations.Contains((combination.Food, c.ActiveSubstance))
                        )
                        .Select(c => (combination.Food, c.ActiveSubstance));
                    result = [.. result.Union(conversions)];
                }
            }
            return result
                .Select(r => new SubstanceAuthorisation{
                    Food = r.Food,
                    Substance = r.Substance
                }).ToList();
        }
    }
}
