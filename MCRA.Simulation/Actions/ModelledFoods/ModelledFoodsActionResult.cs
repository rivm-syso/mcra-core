using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.ModelledFoodsCalculation;

namespace MCRA.Simulation.Actions.ModelledFoods {
    public sealed class ModelledFoodsActionResult : IActionResult {

        public HashSet<Food> ModelledFoods { get; set; }
        public ICollection<ModelledFoodInfo> ModelledFoodsInfos { get; set; }

        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
