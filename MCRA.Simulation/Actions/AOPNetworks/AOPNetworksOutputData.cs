
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.AOPNetworks {
    public class AOPNetworksOutputData : IModuleOutputData {
        public AdverseOutcomePathwayNetwork AdverseOutcomePathwayNetwork { get; set; }
        public ICollection<Effect> RelevantEffects { get; set; }

        public IModuleOutputData Copy() {
            return new AOPNetworksOutputData() {
                AdverseOutcomePathwayNetwork = AdverseOutcomePathwayNetwork,
                RelevantEffects = RelevantEffects
            };
        }
    }
}

