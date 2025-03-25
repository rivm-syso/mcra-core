using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ExposureResponseFunctions {
    public class ExposureResponseFunctionsOutputData : IModuleOutputData {
        public IList<ExposureResponseFunction> ExposureResponseFunctions { get; set; }
        public IModuleOutputData Copy() {
            return new ExposureResponseFunctionsOutputData() {
                ExposureResponseFunctions = ExposureResponseFunctions
            };
        }
    }
}
