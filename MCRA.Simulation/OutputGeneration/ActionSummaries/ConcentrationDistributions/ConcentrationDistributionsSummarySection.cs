using MCRA.Utils.Collections;
using MCRA.Utils.ExtensionMethods;
using MCRA.Data.Compiled.Objects;
using MCRA.General;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ConcentrationDistributionsSummarySection : ActionSummaryBase {
        public List<ConcentrationDistributionsDataRecord> Records { get; set; }

        public void Summarize(
            IDictionary<(Food, Compound), ConcentrationDistribution> concentrationDistributions
        ) {
            Records = concentrationDistributions
                .Select(c => {
                    return new ConcentrationDistributionsDataRecord() {
                        CompoundName = c.Key.Item2.Name,
                        CompoundCode = c.Key.Item2.Code,
                        FoodCode = c.Key.Item1.Code,
                        FoodName = c.Key.Item1.Name,
                        Mean = c.Value.Mean,
                        Cv = c.Value.CV ?? double.NaN,
                        Percentile = c.Value.Percentile ?? double.NaN,
                        Percentage = c.Value.Percentage ?? double.NaN,
                        Limit = c.Value.Limit ?? double.NaN,
                        Unit = c.Value.ConcentrationUnit.GetShortDisplayName()
                    };
                })
                .ToList();
        }
    }
}