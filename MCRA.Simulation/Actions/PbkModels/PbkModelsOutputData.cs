using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.PbkModels {
    public class PbkModelsOutputData : IModuleOutputData {
        public ICollection<KineticModelInstance> KineticModelInstances { get; set; }
        public IModuleOutputData Copy() {
            return new PbkModelsOutputData() {
                KineticModelInstances = KineticModelInstances,
            };
        }
    }
}

