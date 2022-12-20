using MCRA.Utils.Collections;
using MCRA.Data.Compiled.Objects;
using MCRA.Data.Compiled.Wrappers;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SingleValueConcentrationsSummarySection : ActionSummaryBase {

        public double ExceedanceFactionThreshold { get; set; }

        public List<SingleValueConcentrationSummaryRecord> Records { get; set; }

        public void Summarize(
            IDictionary<(Food, Compound), SingleValueConcentrationModel> singleValueConcentrationModels
        ) {
            Records = singleValueConcentrationModels.Values
                .Select(r => {
                    var measuredSubstance = (r as ConvertedSingleValueConcentrationModel)?.MeasuredSingleValueConcentrationModel?.Substance;
                    return new SingleValueConcentrationSummaryRecord() {
                        SubstanceName = r.Substance.Name,
                        SubstanceCode = r.Substance.Code,
                        FoodCode = r.Food.Code,
                        FoodName = r.Food.Name,
                        MeanConcentration = r.MeanConcentration,
                        MedianConcentration = r.GetPercentile(50),
                        HighestConcentration = r.HighestConcentration,
                        Loq = r.Loq,
                        Mrl = r.Mrl,
                        MeasuredSubstanceCode = measuredSubstance?.Code,
                        MeasuredSubstanceName = measuredSubstance?.Name,
                        ConversionFactor = (r as ConvertedSingleValueConcentrationModel)?.ConversionFactor ?? double.NaN
                    };
                })
                .OrderBy(c => c.FoodName, System.StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.SubstanceName, System.StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
