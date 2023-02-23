using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TdsReductionFactorsSection : SummarySection {
        public List<TdsReductionFactorRecord> Records { get; set; }

        public void Summarize(IDictionary<(Food Food, Compound Substance), double> tdsReductionFactors) {
            Records = tdsReductionFactors
                .Where(r => r.Value < 1)
                .Select(record => new TdsReductionFactorRecord() {
                    FoodName = record.Key.Food.Name,
                    FoodCode = record.Key.Food.Code,
                    SubstanceName = record.Key.Substance.Name,
                    SubstanceCode = record.Key.Substance.Code,
                    Factor = record.Value,
                })
                .OrderBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.SubstanceName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
