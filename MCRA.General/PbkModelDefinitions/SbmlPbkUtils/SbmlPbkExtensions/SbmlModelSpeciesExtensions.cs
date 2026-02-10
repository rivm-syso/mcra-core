using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.Sbml {

    public static class SbmlModelSpeciesExtensions {

        public static bool IsMetaboliteSpecies(this SbmlModelSpecies species) {
            var result = species.BqbIsResources?
                .Any(r => r.EndsWith(@"CHEBI:25212", StringComparison.OrdinalIgnoreCase)) ?? false;
            return result;
        }

        public static string GetSubstanceId(this SbmlModelSpecies species) {
            var resource = species.BqbIsResources?
                .FirstOrDefault(r => r.Contains(@"CHEBI")
            );
            if (resource != null) {
                var resourceUri = new Uri(resource);
                return resourceUri.Segments.LastOrDefault();
            } else {
                return null;
            }
        }
    }
}
