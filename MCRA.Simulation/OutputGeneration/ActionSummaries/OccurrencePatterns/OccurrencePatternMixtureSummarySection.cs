using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes the concentrations of modelled foods from input data
    /// </summary>
    public sealed class OccurrencePatternMixtureSummarySection : SummarySection {
        public List<OccurrencePatternMixtureSummaryRecord> Records { get; set; }

        public void Summarize(
            Dictionary<Food, List<MarginalOccurrencePattern>> agriculturalUseMixtures,
            bool summarizeIsAuthorisedUse = true
        ) {
            var flatList = agriculturalUseMixtures.SelectMany(r => r.Value).ToList();
            Summarize(flatList, summarizeIsAuthorisedUse);
        }

        public void Summarize(
            ICollection<MarginalOccurrencePattern> occurrencePatterns,
            bool summarizeIsAuthorisedUse = true
        ) {
            Records = occurrencePatterns
                .Select(r => new OccurrencePatternMixtureSummaryRecord() {
                    FoodCode = r.Food.Code,
                    FoodName = r.Food.Name,
                    SubstanceCodes = r.Compounds.Select(c => c.Code).ToList(),
                    SubstanceNames = r.Compounds.Select(c => c.Name).ToList(),
                    AgriculturalUseFraction = r.OccurrenceFraction,
                    FromAuthorisedUse = summarizeIsAuthorisedUse ? r.AuthorisedUse : null
                })
                .OrderByDescending(r => r.NumberOfSubstances)
                .ThenByDescending(r => r.AgriculturalUseFraction)
                .ThenBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
