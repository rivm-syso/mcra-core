using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration
{
    public sealed class TdsPotentialReductionFactorsSection : SummarySection
    {
        public List<TdsPotentialReductionFactorRecord> Records { get; set; }

        public void Summarize(
            IDictionary<(Food, Compound), ConcentrationDistribution> concentrationDistributions,
            List<string> selectedFoodCodes,
            ConcentrationUnit concentrationUnit
        ) {
            Records = concentrationDistributions
                .Select(c => {
                    var percentile = double.NaN;
                    var percentage = double.NaN;
                    var limit = double.NaN;
                    var reductionFactor = 1F;
                    var inScenario = selectedFoodCodes.Contains(c.Key.Item1.Code) ? "yes" : "no";
                    if (c.Value != null) {
                        var unitCorrection = c.Value.ConcentrationUnit.GetConcentrationUnitMultiplier(concentrationUnit);
                        percentile = unitCorrection * (double)c.Value.Percentile;
                        percentage = (double)c.Value.Percentage;
                        limit = unitCorrection * (double)c.Value.Limit;
                        reductionFactor = (float)limit / (float)percentile;
                    }
                    return new TdsPotentialReductionFactorRecord() {
                        CompoundName = c.Key.Item2.Name,
                        CompoundCode = c.Key.Item2.Code,
                        FoodCode = c.Key.Item1.Code,
                        FoodName = c.Key.Item1.Name,
                        ReductionFactor = reductionFactor,
                        Percentile = percentile,
                        Percentage = percentage,
                        Limit = limit,
                        InScenario = inScenario,
                    };
                })
                .Where(c => c.ReductionFactor < 1)
                .OrderBy(c => c.FoodName, System.StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.CompoundName, System.StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}