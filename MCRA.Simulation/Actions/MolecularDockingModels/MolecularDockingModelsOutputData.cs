
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.MolecularDockingModels {
    public class MolecularDockingModelsOutputData : IModuleOutputData {
        public ICollection<MolecularDockingModel> MolecularDockingModels { get; set; }
        public IModuleOutputData Copy() {
            return new MolecularDockingModelsOutputData() {
                MolecularDockingModels = MolecularDockingModels,
            };
        }
    }
}

