
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.Populations {
    public class PopulationsOutputData : IModuleOutputData {
        public Population SelectedPopulation { get; set; }
        public IModuleOutputData Copy() {
            return new PopulationsOutputData() {
                SelectedPopulation = SelectedPopulation,
            };
        }
    }
}

