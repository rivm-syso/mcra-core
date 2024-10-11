using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.MockDataGenerators {
    /// <summary>
    /// Class for generating mock effects
    /// </summary>
    public static class MockEffectsGenerator {
        /// <summary>
        /// Creates a mock effect
        /// </summary>
        /// <returns></returns>
        public static Effect Create() {
            var effect = new Effect() {
                Code = $"EffectX",
                Name = $"EffectX"
            };
            return effect;
        }

        /// <summary>
        /// Creates a list of effects
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static List<Effect> Create(int number) {
            var effects = new List<Effect>();
            for (int i = 0; i < number; i++) {
                var effect = new Effect() {
                    Code = $"Effect {i}",
                    Name = $"Effect {i}",
                    AOPWikiIds = "AOPWikiIds",
                    BiologicalOrganisationType = BiologicalOrganisationType.Organ,
                    Description = "Description",
                    IsAChEInhibitor = false,
                    IsGenotoxic = false,
                    IsNonGenotoxicCarcinogenic = false,
                    KeyEventAction = "KeyEventAction",
                    KeyEventCell = "KeyEventCell",
                    KeyEventObject = "KeyEventObject",
                    KeyEventOrgan = "KeyEventOrgan",
                    KeyEventProcess = "KeyEventProcess",
                    Reference = "Reference"
                };
                effects.Add(effect);
            }
            return effects;
        }
    }
}
