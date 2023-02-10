
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.DoseResponseModels {
    public class DoseResponseModelsOutputData : IModuleOutputData {
        public ICollection<DoseResponseModel> DoseResponseModels { get; set; }
        public IModuleOutputData Copy() {
            return new DoseResponseModelsOutputData() {
                DoseResponseModels = DoseResponseModels
            };
        }
    }
}

