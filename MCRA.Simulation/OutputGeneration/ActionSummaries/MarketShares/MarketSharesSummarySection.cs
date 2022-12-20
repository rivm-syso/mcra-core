using MCRA.Data.Compiled.Objects;
using System.Collections.Generic;
using System.Linq;


namespace MCRA.Simulation.OutputGeneration {
    public sealed class MarketSharesSummarySection : SummarySection {

        public List<MarketShareRecord> Records { get; set; }

        public void Summarize(ICollection<MarketShare> marketShares) {
            Records = marketShares.Select(c => new MarketShareRecord() {
                FoodName = c.Food.Name,
                FoodCode = c.Food.Code,
                Proportion = c.Percentage/100,
                Brandloyalty = c.BrandLoyalty,
            }).ToList();
        }
    }
}