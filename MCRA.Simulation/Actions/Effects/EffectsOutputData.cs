
using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.Effects {
    public class EffectsOutputData : IModuleOutputData {
        public ICollection<Effect> AllEffects { get; set; }
        public Effect SelectedEffect { get; set; }
        public IModuleOutputData Copy() {
            return new EffectsOutputData() {
                AllEffects = AllEffects,
                SelectedEffect = SelectedEffect
            };
        }
    }
}

