using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.BackgroundFoodConcentrations {
    public class BackgroundFoodConcentrationsOutputData : IModuleOutputData {
        public ICollection<BackgroundFoodConcentration> BackgroundFoodConcentrations { get; set; }
        public IModuleOutputData Copy() {
            return new BackgroundFoodConcentrationsOutputData() {
                BackgroundFoodConcentrations = BackgroundFoodConcentrations,
            };
        }
    }
}

