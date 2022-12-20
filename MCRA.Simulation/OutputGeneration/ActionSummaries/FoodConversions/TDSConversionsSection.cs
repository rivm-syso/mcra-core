using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class TDSConversionsSection : SummarySection {

        /// <summary>
        /// Summary of conversion results
        /// </summary>
        public List<TDSConversionSummaryRecord> FoodConversionSummaryRecords = new List<TDSConversionSummaryRecord>();

        public void Summarize(ILookup<Food, TDSFoodSampleComposition> tdsCompositionsLookup) {
            foreach (var record in tdsCompositionsLookup) {
                foreach (var composition in record) {
                    var conversionRecord = new TDSConversionSummaryRecord() {
                        CompoundCode = string.Empty,
                        CompoundName = string.Empty,
                        FoodAsEatenCode = composition.Food.Code,
                        FoodAsEatenName = composition.Food.Name,
                        FoodAsMeasuredCode = composition.TDSFood.Code,
                        FoodAsMeasuredName = composition.TDSFood.Name,
                        Regionality = composition.Regionality,
                        Seasonality = composition.Seasonality,
                        NumberOfSamples = composition.PooledAmount,
                        Description = composition.Description,
                    };
                    FoodConversionSummaryRecords.Add(conversionRecord);
                }
            }
            FoodConversionSummaryRecords.TrimExcess();
        }
    }
}
