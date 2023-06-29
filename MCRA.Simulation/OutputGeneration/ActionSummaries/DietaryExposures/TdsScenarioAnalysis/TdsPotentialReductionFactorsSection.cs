using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TdsPotentialReductionFactorsSection : SummarySection {
        public List<TdsPotentialReductionFactorRecord> Records { get; set; }

        public void Summarize(
            IDictionary<(Food Food, Compound Substance), ConcentrationDistribution> concentrationDistributions,
            List<string> selectedFoodCodes,
            ConcentrationUnit concentrationUnit
        ) {
            Records = concentrationDistributions
                .Select(c => {
                    var percentile = double.NaN;
                    var percentage = double.NaN;
                    var limit = double.NaN;
                    var reductionFactor = 1F;
                    var inScenario = selectedFoodCodes.Contains(c.Key.Food.Code) ? "yes" : "no";
                    if (c.Value != null) {
                        var unitCorrection = c.Value.ConcentrationUnit.GetConcentrationUnitMultiplier(concentrationUnit);
                        percentile = unitCorrection * c.Value.Percentile.Value;
                        percentage = c.Value.Percentage.Value;
                        limit = unitCorrection * c.Value.Limit.Value;
                        reductionFactor = (float)limit / (float)percentile;
                    }
                    return new TdsPotentialReductionFactorRecord() {
                        CompoundName = c.Key.Substance.Name,
                        CompoundCode = c.Key.Substance.Code,
                        FoodCode = c.Key.Food.Code,
                        FoodName = c.Key.Food.Name,
                        ReductionFactor = reductionFactor,
                        Percentile = percentile,
                        Percentage = percentage,
                        Limit = limit,
                        InScenario = inScenario,
                    };
                })
                .Where(c => c.ReductionFactor < 1)
                .OrderBy(c => c.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.CompoundName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.CompoundCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}