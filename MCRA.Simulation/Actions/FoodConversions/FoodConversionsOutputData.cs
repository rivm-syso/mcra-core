using MCRA.Simulation.Objects;
using MCRA.Simulation.Action;

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

