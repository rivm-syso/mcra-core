using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;
using MCRA.Simulation.Calculators.ExposureResponseFunctions;

namespace MCRA.Simulation.Actions.ExposureResponseFunctions {
    public class ExposureResponseFunctionsOutputData : IModuleOutputData {
        public IList<ExposureResponseFunction> ExposureResponseFunctions { get; set; }

        public ICollection<IExposureResponseFunctionModel> ExposureResponseFunctionModels { get; set; }

        public IModuleOutputData Copy() {
            return new ExposureResponseFunctionsOutputData() {
                ExposureResponseFunctions = ExposureResponseFunctions,
                ExposureResponseFunctionModels = ExposureResponseFunctionModels
            };
        }
    }
}
