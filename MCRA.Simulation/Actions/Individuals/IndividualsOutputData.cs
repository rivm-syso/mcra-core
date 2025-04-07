using MCRA.Data.Compiled.Wrappers;
using MCRA.Simulation.Action;

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

