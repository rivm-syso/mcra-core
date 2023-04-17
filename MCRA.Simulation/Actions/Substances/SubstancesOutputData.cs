
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.Substances {
    public class SubstancesOutputData : IModuleOutputData {
        public ICollection<Compound> AllCompounds { get; set; }
        
        public IModuleOutputData Copy() {
            return new SubstancesOutputData() {
                AllCompounds = AllCompounds
            };
        }
    }
}

