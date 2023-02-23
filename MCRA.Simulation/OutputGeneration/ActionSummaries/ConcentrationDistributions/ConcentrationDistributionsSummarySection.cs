using MCRA.Data.Compiled.Objects;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ConcentrationDistributionsSummarySection : ActionSummaryBase {
        public List<ConcentrationDistributionsDataRecord> Records { get; set; }

        public void Summarize(
            IDictionary<(Food Food, Compound Substance), ConcentrationDistribution> concentrationDistributions
        ) {
            Records = concentrationDistributions
                .Select(c => {
                    return new ConcentrationDistributionsDataRecord() {
                        CompoundName = c.Key.Substance.Name,
                        CompoundCode = c.Key.Substance.Code,
                        FoodCode = c.Key.Food.Code,
                        FoodName = c.Key.Food.Name,
                        Mean = c.Value.Mean,
                        Cv = c.Value.CV ?? double.NaN,
                        Percentile = c.Value.Percentile ?? double.NaN,
                        Percentage = c.Value.Percentage ?? double.NaN,
                        Limit = c.Value.Limit ?? double.NaN,
                        Unit = c.Value.ConcentrationUnit.GetShortDisplayName()
                    };
                }).ToList();
        }
    }
}