using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TdsReductionFactorsSection : SummarySection {
        public List<TdsReductionFactorRecord> Records { get; set; }

        public void Summarize(IDictionary<(Food, Compound), double> tdsReductionFactors) {
            Records = tdsReductionFactors
                .Where(r => r.Value < 1)
                .Select(record => new TdsReductionFactorRecord() {
                    FoodName = record.Key.Item1.Name,
                    FoodCode = record.Key.Item1.Code,
                    SubstanceName = record.Key.Item2.Name,
                    SubstanceCode = record.Key.Item2.Code,
                    Factor = record.Value,
                })
                .OrderBy(r => r.FoodName, System.StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.SubstanceName, System.StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
