using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.Calculators.IntraSpeciesConversion {
    public static class IntraSpeciesFactorExtensions {

        public static IntraSpeciesFactorModel Get(
            this IDictionary<(Effect, Compound), IntraSpeciesFactorModel> intraSpeciesFactors,
            Effect effect,
            Compound substance = null
        ) {
            IntraSpeciesFactorModel model = null;
            if (!intraSpeciesFactors?.Any() ?? true) {
                return model;
            } else if (intraSpeciesFactors.TryGetValue((effect, substance), out model)) {
                return model;
            } else if (intraSpeciesFactors.TryGetValue((null, substance), out model)) {
                return model;
            } else if (intraSpeciesFactors.TryGetValue((effect, null), out model)) {
                return model;
            } else if (intraSpeciesFactors.TryGetValue((null, null), out model)) {
                return model;
            }
            return model;
        }
    }
}
