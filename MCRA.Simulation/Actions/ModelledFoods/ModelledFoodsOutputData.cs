
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ModelledFoodsCalculation;

namespace MCRA.Simulation.Actions.ModelledFoods {
    public class ModelledFoodsOutputData : IModuleOutputData {
        public ILookup<Food, ModelledFoodInfo> ModelledFoodInfos { get; set; }
        public IModuleOutputData Copy() {
            return new ModelledFoodsOutputData() {
                ModelledFoodInfos = ModelledFoodInfos
            };
        }
    }
}

