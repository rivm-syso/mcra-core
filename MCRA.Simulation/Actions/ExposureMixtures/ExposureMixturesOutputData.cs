
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.ExposureMixtures {
    public class ExposureMixturesOutputData : IModuleOutputData {
        public IModuleOutputData Copy() {
            return new ExposureMixturesOutputData() {
            };
        }
    }
}

