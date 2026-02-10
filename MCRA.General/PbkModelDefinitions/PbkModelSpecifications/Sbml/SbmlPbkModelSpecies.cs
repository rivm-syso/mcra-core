using MCRA.Utils.Sbml.Objects;

namespace MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml {
    public class SbmlPbkModelSpecies {

        /// <summary>
        /// The SBML model species definition.
        /// </summary>
        public SbmlModelSpecies SbmlModelSpecies { get; set; }

        /// <summary>
        /// Identifier of the model species.
        /// </summary>
        public string Id => SbmlModelSpecies.Id;

        /// <summary>
        /// The name of the model species.
        /// </summary>
        public string Name => SbmlModelSpecies.Name;

        /// <summary>
        /// Identifier of the substance represented by this species.
        /// </summary>
        public string IdSubstance => GetSubstanceId();

        /// <summary>
        /// Substance amount unit.
        /// </summary>
        public SubstanceAmountUnit SubstanceAmountUnit { get; set; }

        /// <summary>
        /// The compartment in which the species is located.
        /// </summary>
        public SbmlPbkModelCompartment Compartment { get; set; }

        public bool IsMetaboliteSpecies() {
            var result = SbmlModelSpecies.BqbIsResources?
                .Any(r => r.EndsWith(@"CHEBI:25212", StringComparison.OrdinalIgnoreCase)) ?? false;
            return result;
        }

        public string GetSubstanceId() {
            var resource = SbmlModelSpecies.BqbIsResources?
                .FirstOrDefault(r => r.Contains(@"CHEBI"));
            if (resource != null) {
                var resourceUri = new Uri(resource);
                return resourceUri.Segments.LastOrDefault();
            } else {
                return null;
            }
        }
    }
}
