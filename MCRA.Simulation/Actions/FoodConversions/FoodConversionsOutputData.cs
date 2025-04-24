using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.FoodConversionCalculation;

namespace MCRA.Simulation.Actions.FoodConversions {
    public class FoodConversionsOutputData : IModuleOutputData {
        public ICollection<FoodConversionResult> FoodConversionResults { get; set; }
        public IModuleOutputData Copy() {
            return new FoodConversionsOutputData() {
                FoodConversionResults = FoodConversionResults,
            };
        }
    }
}

