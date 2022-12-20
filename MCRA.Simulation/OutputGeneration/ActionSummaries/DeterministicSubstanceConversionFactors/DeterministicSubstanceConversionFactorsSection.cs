using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DeterministicSubstanceConversionFactorsSection : SummarySection {
        public List<DeterministicSubstanceConversionFactorRecord> Records { get; set; }

        public void Summarize(ICollection<DeterministicSubstanceConversionFactor> deterministicSubstanceConversionFactor) {
            Records = deterministicSubstanceConversionFactor?
                .Select(r => new DeterministicSubstanceConversionFactorRecord() {
                    MeasuredSubstanceCode = r.MeasuredSubstance.Code,
                    MeasuredSubstanceName = r.MeasuredSubstance.Name,
                    ActiveSubstanceCode = r.ActiveSubstance.Code,
                    ActiveSubstanceName = r.ActiveSubstance.Name,
                    FoodCode = r.Food?.Code ?? string.Empty,
                    FoodName = r.Food?.Name ?? string.Empty,
                    ConversionFactor = r.ConversionFactor,
                    Reference = r.Reference,
                })
                .ToList();
        }
    }
}
