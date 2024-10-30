
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ExposureEffectFunctions {
    public class ExposureEffectFunctionsOutputData : IModuleOutputData {
        public IList<ExposureEffectFunction> ExposureEffectFunctions { get; set; }
        public IModuleOutputData Copy() {
            return new ExposureEffectFunctionsOutputData() {
                ExposureEffectFunctions = ExposureEffectFunctions
            };
        }
    }
}
