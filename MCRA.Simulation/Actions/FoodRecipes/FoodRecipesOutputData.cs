
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.FoodRecipes {
    public class FoodRecipesOutputData : IModuleOutputData {
        public ICollection<FoodTranslation> FoodRecipes { get; set; }
        public IModuleOutputData Copy() {
            return new FoodRecipesOutputData() {
                FoodRecipes = FoodRecipes,
            };
        }
    }
}

