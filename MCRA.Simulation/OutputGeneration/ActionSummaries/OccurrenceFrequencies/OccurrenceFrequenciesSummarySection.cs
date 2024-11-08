using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OccurrenceFrequenciesSummarySection : SummarySection {
        public override bool SaveTemporaryData => true;

        public List<AgriculturalUseByFoodSubstanceSummaryRecord> Records { get; set; }

        public void Summarize(IDictionary<(Food Food, Compound Substance), OccurrenceFraction> agriculturalUseInfo, IDictionary<(Food, Compound), SubstanceAuthorisation> substanceAuthorisations) {
            Records = agriculturalUseInfo
                .SelectMany(c => c.Value.GetSummarizedLocationAgriculturalUses().OrderBy(r => r.IsUndefinedLocation),
                    (fau, cau) => new AgriculturalUseByFoodSubstanceSummaryRecord() {
                        FoodCode = fau.Key.Food.Code,
                        FoodName = fau.Key.Food.Name,
                        CompoundName = fau.Key.Substance.Name,
                        CompoundCode = fau.Key.Substance.Code,
                        AgriculturalUseFraction = cau.OccurrenceFraction,
                        Location = string.IsNullOrEmpty(cau.Location) ? "General" : cau.Location,
                        IsAuthorised = substanceAuthorisations?.ContainsKey(fau.Key)
                    }
                )
                .OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.CompoundName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.CompoundCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.Location, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.Location == "General")
                .ToList();
        }

        public void SummarizeUncertain(IDictionary<(Food Food, Compound Substance), OccurrenceFraction> agriculturalUseInfo) {
            var modelsLookup = Records.ToDictionary(r => (r.FoodCode, r.CompoundCode, r.Location));
            var records = agriculturalUseInfo
                .SelectMany(c => c.Value.GetSummarizedLocationAgriculturalUses(),
                    (fau, cau) => new {
                        FoodCode = fau.Key.Food.Code,
                        CompoundCode = fau.Key.Substance.Code,
                        Location = string.IsNullOrEmpty(cau.Location) ? "General" : cau.Location,
                        AgriculturalUseFraction = cau.OccurrenceFraction
                    })
                .ToList();
            foreach (var record in records) {
                var location = string.IsNullOrEmpty(record.Location) ? "General" : record.Location;
                modelsLookup.TryGetValue((record.FoodCode, record.CompoundCode, record.Location), out var model);
                if (model != null) {
                    if (model.AgriculturalUseFractionUncertaintyValues == null) {
                        model.AgriculturalUseFractionUncertaintyValues = [];
                    }
                    model.AgriculturalUseFractionUncertaintyValues.Add(record.AgriculturalUseFraction);
                }
            }
        }
    }
}
