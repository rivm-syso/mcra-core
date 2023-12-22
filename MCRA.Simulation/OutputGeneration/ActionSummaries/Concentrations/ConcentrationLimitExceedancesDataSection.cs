using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConcentrationLimitExceedancesDataSection : ActionSummarySectionBase {

        public double ExceedanceFactionThreshold { get; set; }

        public List<ConcentrationLimitExceedanceDataRecord> Records { get; set; }

        public void Summarize(
            ICollection<ConcentrationLimit> limits,
            ILookup<Food, FoodSample> foodSamplesLookup,
            ConcentrationUnit concentrationUnit,
            double exceedanceFactionThreshold
        ) {
            ExceedanceFactionThreshold = exceedanceFactionThreshold;
            Records = limits
                .Where(r => !double.IsNaN(r.Limit))
                .GroupBy(r => r.Food)
                .AsParallel()
                .SelectMany(g => {
                    var food = g.Key;
                    var foodSamples = foodSamplesLookup.Contains(food) ? foodSamplesLookup[food].ToList() : new List<FoodSample>();
                    var result = g.Select(limit => {
                        var samplesInAnalyticalScope = foodSamples
                            .Where(s => s.SampleAnalyses.Any(sa => sa.Concentrations.ContainsKey(limit.Compound))
                                || (s.SampleAnalyses.Any(sa => sa.AnalyticalMethod?.AnalyticalMethodCompounds?.ContainsKey(limit.Compound) ?? false)))
                            .ToList();

                        var samplesExceedingLimit = samplesInAnalyticalScope
                            .Where(s => sampleExceedsLimit(s, limit, exceedanceFactionThreshold))
                            .ToList();
                        var unitCorrection = limit.ConcentrationUnit.GetConcentrationUnitMultiplier(concentrationUnit);
                        return new ConcentrationLimitExceedanceDataRecord() {
                            SubstanceName = limit.Compound.Name,
                            SubstanceCode = limit.Compound.Code,
                            FoodCode = limit.Food.Code,
                            FoodName = limit.Food.Name,
                            LimitValue = unitCorrection * limit.Limit,
                            TotalNumberOfAnalysedSamples = samplesInAnalyticalScope.Count,
                            NumberOfSamplesExceedingLimit = samplesExceedingLimit.Count,
                            FractionOfTotal = (double)samplesExceedingLimit.Count / samplesInAnalyticalScope.Count
                        };
                    });
                    return result;
                })
                .Where(r => !double.IsNaN(r.FractionOfTotal) && r.FractionOfTotal > 0)
                .OrderBy(c => c.FoodName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.FoodCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.SubstanceName, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.SubstanceCode, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static bool sampleExceedsLimit(
            FoodSample foodSample,
            ConcentrationLimit limit,
            double exceedanceFactionThreshold
        ) {
            foreach (var sampleAnalysis in foodSample.SampleAnalyses) {
                if (sampleAnalysis.Concentrations.ContainsKey(limit.Compound)) {
                    if (sampleAnalysis.AnalyticalMethod.AnalyticalMethodCompounds.TryGetValue(limit.Compound, out var amc)) {
                        var concentrationUnit = amc.GetConcentrationUnit();
                        var concentrationUnitCorrection = concentrationUnit.GetConcentrationUnitMultiplier(limit.ConcentrationUnit);
                        if (concentrationUnitCorrection * sampleAnalysis.Concentrations[limit.Compound].Concentration > exceedanceFactionThreshold * limit.Limit) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}