namespace MCRA.Utils.Sbml.Objects {
    public class SbmlModel {

        public string Id { get; set; }

        public string Name { get; set; }

        public SbmlTimeUnit TimeUnit { get; set; }

        public List<SbmlModelCompartment> Compartments { get; set; }

        public List<SbmlModelParameter> Parameters { get; set; }

        public List<SbmlModelSpecies> Species { get; set; }

        public List<SbmlModelAssignmentRule> AssignmentRules { get; set; }

        public List<SbmlReaction> Reactions { get; set; }

        public Dictionary<string, SbmlUnitDefinition> UnitDefinitions { get; set; }

        public List<SbmlModelParameter> GetAssignableParameters() {
            var result = Parameters
                .Where(r => !AssignmentRules.Any(ar => ar.Variable == r.Id))
                .ToList();
            return result;
        }
    }
}
