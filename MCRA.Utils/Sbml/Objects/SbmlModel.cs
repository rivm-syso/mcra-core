namespace MCRA.Utils.Sbml.Objects {
    public class SbmlModel {

        public string Id { get; set; }

        public string Name { get; set; }

        public string TimeUnits { get; set; }

        public string VolumeUnits { get; set; }

        public string SubstancesUnits { get; set; }

        public Dictionary<string, SbmlModelCompartment> Compartments { get; set; }

        public Dictionary<string, SbmlModelParameter> Parameters { get; set; }

        public Dictionary<string, SbmlModelSpecies> Species { get; set; }

        public List<SbmlModelAssignmentRule> AssignmentRules { get; set; }

        public List<SbmlReaction> Reactions { get; set; }

        public Dictionary<string, SbmlUnitDefinition> UnitDefinitions { get; set; }

        public List<SbmlModelParameter> GetAssignableParameters() {
            var result = Parameters.Values
                .Where(r => !AssignmentRules.Any(ar => ar.Variable == r.Id))
                .ToList();
            return result;
        }
    }
}
