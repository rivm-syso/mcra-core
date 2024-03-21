using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {

    public static class SbmlModelSpeciesExtensions {

        public static bool IsMetaboliteSpecies(this SbmlModelSpecies species) {
            var result = species.BqbIsResources?
                .Any(r => r.EndsWith(@"CHEBI:25212", StringComparison.OrdinalIgnoreCase)) ?? false;
            return result;
        }
    }
}
