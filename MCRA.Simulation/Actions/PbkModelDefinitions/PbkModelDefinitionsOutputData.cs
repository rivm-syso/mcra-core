using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.PbkModelDefinitions {
    public class PbkModelDefinitionsOutputData : IModuleOutputData {
        public ICollection<PbkModelDefinition> PbkModelDefinitions { get; set; }
        public IModuleOutputData Copy() {
            return new PbkModelDefinitionsOutputData() {
                PbkModelDefinitions = PbkModelDefinitions,
            };
        }
    }
}
