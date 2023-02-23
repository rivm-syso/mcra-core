using MCRA.Data.Compiled.Wrappers;
using System.Collections.Generic;
using System.Linq;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ModelledFoodMarketShareDataSection : SummarySection{

        public List<ModelledFoodMarketShareRecord> Records { get; set; }

        public void Summarize(ICollection<ConsumptionsByModelledFood> consumptionsPerModelledFood) {
            var modelledFoodMarketShareRecords = new List<ModelledFoodMarketShareRecord>();
            var marketShareFoods = consumptionsPerModelledFood
                .Where(c => c.IsBrand)
                .Select(c => c.FoodAsMeasured)
                .Distinct();

            foreach (var marketShareFood in marketShareFoods) {
                var marketShareRecord = consumptionsPerModelledFood
                     .Where(c => c.IsBrand && c.FoodAsMeasured == marketShareFood)
                     .Select(c => new ModelledFoodMarketShareRecord {
                         ConsumedFoodCode = c.FoodConsumption.Food.Code,
                         ConsumedFoodName = c.FoodConsumption.Food.Name,
                         FoodCode = c.FoodAsMeasured.Code,
                         FoodName = c.FoodAsMeasured.Name,
                         MarketShare = c.FoodAsMeasured.MarketShare.Percentage / 100,
                     })
                     .First();
                modelledFoodMarketShareRecords.Add(marketShareRecord);
            }

            Records =  modelledFoodMarketShareRecords
                .OrderBy(c => c.ConsumedFoodCode, StringComparer.OrdinalIgnoreCase)
                .ThenBy(c => c.MarketShare)
                .ToList();
            Records.TrimExcess();
        }
    }
}
