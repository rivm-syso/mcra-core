using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class FoodSubstanceConversionFactorsSummarySection : SummarySection {

        public List<FoodSubstanceConversionFactorRecord> Records { get; set; }
        public void Summarize(
            ICollection<DeterministicSubstanceConversionFactor> conversionFactors
        ) {
            Records = [.. conversionFactors.Select(c => new FoodSubstanceConversionFactorRecord() {
                ActiveSubstanceCode = c.ActiveSubstance.Code,
                ActiveSubstanceName = c.ActiveSubstance.Name,
                MeasuredSubstanceName= c.MeasuredSubstance.Name,
                MeasuredSubstanceCode= c.MeasuredSubstance.Code,
                FoodCode = c.Food.Code,
                FoodName = c.Food.Name,
                ConversionFactor = c.ConversionFactor,
                Reference = c.Reference
            })];
        }
    }
}
