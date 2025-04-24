using MCRA.Simulation.Action;
using MCRA.Simulation.Action.UncertaintyFactorial;
using MCRA.Simulation.Calculators.FoodConversionCalculation;

namespace MCRA.Simulation.Actions.FoodConversions {
    public sealed class FoodConversionActionResult : IActionResult {
        public ICollection<FoodConversionResult> FoodConversionResults { get; set; }
        public ICollection<FoodConversionResult> FailedFoodConversionResults { get; set; }
        public IUncertaintyFactorialResult FactorialResult { get; set; }
    }
}
