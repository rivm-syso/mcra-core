using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {

    /// <summary>
    /// Summarizes deterministic consumption estimates
    /// </summary>
    public sealed class SingleValueConsumptionsDataSummarySection : SummarySection {

        public List<SingleValueConsumptionsDataSummaryRecord> Records { get; set; }

        /// <summary>
        /// Summarizes the records.
        /// </summary>
        /// <param name="PopulationConsumptionSingleValues"></param>
        /// <param name=""></param>
        public void Summarize(
            ICollection<PopulationConsumptionSingleValue> PopulationConsumptionSingleValues
        ) {
            Records = PopulationConsumptionSingleValues
                .Select(c => new SingleValueConsumptionsDataSummaryRecord() {
                    FoodName = c.Food.Name,
                    FoodCode = c.Food.Code,
                    Amount = c.ConsumptionAmount,
                    Reference = c.Reference,
                    ConsumptionType = c.ValueType.GetDisplayName(),
                    Unit = c.ConsumptionUnit.GetShortDisplayName(),
                    Percentile = (double)(c.Percentile ?? double.NaN),
                    PopulationName =  c.Population?.Name ?? string.Empty
                })
                .OrderBy(r => r.PopulationName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(r => r.FoodName, StringComparer.OrdinalIgnoreCase)
                .ToList();
            Records.TrimExcess();
        }
    }
}
