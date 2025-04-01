using MCRA.Simulation.Action;
using MCRA.Simulation.Objects;

namespace MCRA.Simulation.Actions.Individuals {
    public class IndividualsOutputData : IModuleOutputData {

        public ICollection<IIndividualDay> Individuals { get; set; }

        public IModuleOutputData Copy() {
            return new IndividualsOutputData() {
                Individuals = Individuals
            };
        }
    }
}

