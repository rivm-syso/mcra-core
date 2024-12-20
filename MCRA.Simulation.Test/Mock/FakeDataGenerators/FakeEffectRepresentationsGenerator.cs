﻿using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.Test.Mock.FakeDataGenerators {
    /// <summary>
    /// Class for generating mock effect representations
    /// </summary>
    public static class FakeEffectRepresentationsGenerator {
        /// <summary>
        /// Creates a list of effect representations
        /// </summary>
        /// <param name="effects"></param>
        /// <param name="responses"></param>
        /// <returns></returns>
        public static List<EffectRepresentation> Create(
            ICollection<Effect> effects,
            ICollection<Response> responses
        ) {
            var effectRepresentations = new List<EffectRepresentation>();
            foreach (var effect in effects) {
                foreach (var response in responses) {
                    var effectRepresentation = new EffectRepresentation() {
                        Effect = effect,
                        Response = response,
                        BenchmarkResponseType = (response.ResponseType == ResponseType.Quantal ? BenchmarkResponseType.ExtraRisk : BenchmarkResponseType.Factor),
                        BenchmarkResponse = response.ResponseType == ResponseType.Quantal ? 0.05 : 0.95,
                    };
                    effectRepresentations.Add(effectRepresentation);
                }

            }
            return effectRepresentations;
        }
    }
}
