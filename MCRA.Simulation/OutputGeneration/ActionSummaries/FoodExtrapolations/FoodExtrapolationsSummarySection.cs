using MCRA.Data.Compiled.Objects;


namespace MCRA.Simulation.OutputGeneration {
    public sealed class FoodExtrapolationsSummarySection : SummarySection {

        public List<FoodExtrapolationsSummaryRecord> Records { get; set; }

        public void Summarize(IDictionary<Food, ICollection<Food>> foodExtrapolations) {
            Records = foodExtrapolations
                .SelectMany(fe => fe.Value, (fe, r) => new FoodExtrapolationsSummaryRecord() {
                    FoodFromCode = fe.Key.Code,
                    FoodFromName = fe.Key.Name,
                    FoodToCode = r.Code,
                    FoodToName = r.Name
                })
                .OrderBy(c => c.FoodToName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}