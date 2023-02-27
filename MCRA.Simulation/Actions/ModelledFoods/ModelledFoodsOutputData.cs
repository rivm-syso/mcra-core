
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ModelledFoodsCalculation;

namespace MCRA.Simulation.Actions.ModelledFoods {
    public class ModelledFoodsOutputData : IModuleOutputData {
        public ICollection<Food> ModelledFoods { get; set; }
        public ILookup<Food, ModelledFoodInfo> ModelledFoodInfos { get; set; }
        public IModuleOutputData Copy() {
            return new ModelledFoodsOutputData() {
                ModelledFoods = ModelledFoods,
                ModelledFoodInfos = ModelledFoodInfos
            };
        }
    }
}

