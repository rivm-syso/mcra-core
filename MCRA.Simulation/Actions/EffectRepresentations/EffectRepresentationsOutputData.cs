
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.EffectRepresentations {
    public class EffectRepresentationsOutputData : IModuleOutputData {
        public ILookup<Effect, EffectRepresentation> AllEffectRepresentations { get; set; }
        public ICollection<EffectRepresentation> FocalEffectRepresentations { get; set; }
        public IModuleOutputData Copy() {
            return new EffectRepresentationsOutputData() {
                AllEffectRepresentations = AllEffectRepresentations,
                FocalEffectRepresentations = FocalEffectRepresentations
            };
        }
    }
}

