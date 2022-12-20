using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Calculators.OccurrencePatternsCalculation;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OccurrenceFrequenciesSummarySection : SummarySection {
        public override bool SaveTemporaryData => true;

        public List<AgriculturalUseByFoodSubstanceSummaryRecord> Records { get; set; }

        public void Summarize(IDictionary<(Food, Compound), OccurrenceFraction> agriculturalUseInfo, IDictionary<(Food, Compound), SubstanceAuthorisation> substanceAuthorisations) {
            Records = agriculturalUseInfo
                .SelectMany(c => c.Value.GetSummarizedLocationAgriculturalUses().OrderBy(r => r.IsUndefinedLocation),
                    (fau, cau) => new AgriculturalUseByFoodSubstanceSummaryRecord() {
                        FoodCode = fau.Key.Item1.Code,
                        FoodName = fau.Key.Item1.Name,
                        CompoundName = fau.Key.Item2.Name,
                        CompoundCode = fau.Key.Item2.Code,
                        AgriculturalUseFraction = cau.OccurrenceFraction,
                        Location = string.IsNullOrEmpty(cau.Location) ? "General" : cau.Location,
                        IsAuthorised = substanceAuthorisations?.ContainsKey(fau.Key)
                    }
                )
                .OrderBy(r => r.FoodName, System.StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.CompoundName, System.StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.Location, System.StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.Location == "General")
                .ToList();
        }

        public void SummarizeUncertain(IDictionary<(Food, Compound), OccurrenceFraction> agriculturalUseInfo) {
            var modelsLookup = Records.ToDictionary(r => (r.FoodCode, r.CompoundCode, r.Location));
            var records = agriculturalUseInfo
                .SelectMany(c => (c.Value as OccurrenceFraction).GetSummarizedLocationAgriculturalUses(),
                    (fau, cau) => new {
                        FoodCode = fau.Key.Item1.Code,
                        CompoundCode = fau.Key.Item2.Code,
                        Location = string.IsNullOrEmpty(cau.Location) ? "General" : cau.Location,
                        AgriculturalUseFraction = cau.OccurrenceFraction
                    })
                .ToList();
            foreach (var record in records) {
                var location = string.IsNullOrEmpty(record.Location) ? "General" : record.Location;
                modelsLookup.TryGetValue((record.FoodCode, record.CompoundCode, record.Location), out var model);
                if (model != null) {
                    if (model.AgriculturalUseFractionUncertaintyValues == null) {
                        model.AgriculturalUseFractionUncertaintyValues = new List<double>();
                    }
                    model.AgriculturalUseFractionUncertaintyValues.Add(record.AgriculturalUseFraction);
                }
            }
        }
    }
}
