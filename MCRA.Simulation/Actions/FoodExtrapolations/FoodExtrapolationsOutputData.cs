
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.FoodExtrapolations {
    public class FoodExtrapolationsOutputData : IModuleOutputData {
        public IDictionary<Food, ICollection<Food>> FoodExtrapolations { get; set; }
        public IModuleOutputData Copy() {
            return new FoodExtrapolationsOutputData() {
                FoodExtrapolations = FoodExtrapolations
            };
        }
    }
}

