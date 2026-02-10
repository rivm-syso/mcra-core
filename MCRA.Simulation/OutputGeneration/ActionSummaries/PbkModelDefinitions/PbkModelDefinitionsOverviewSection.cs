using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.General.PbkModelDefinitions.PbkModelSpecifications.Sbml;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PbkModelDefinitionsOverviewSection : SummarySection {

        public List<PbkModelDefinitionSummaryRecord> Records { get; set; } = [];

        public void Summarize(ICollection<PbkModelDefinition> pbkModelDefinitions) {
            foreach (var definition in pbkModelDefinitions) {
                var def = definition.KineticModelDefinition;
                if (def is SbmlPbkModelSpecification) {
                    // We only include SBML PBK models in this table and skip the hard-coded DeSolve models
                    var inputs = (def as SbmlPbkModelSpecification).GetRouteInputSpecies();
                    inputs.TryGetValue(ExposureRoute.Oral, out var oral);
                    inputs.TryGetValue(ExposureRoute.Dermal, out var dermal);
                    inputs.TryGetValue(ExposureRoute.Inhalation, out var inhalation);
                    var exposureRouteStrings = inputs.Keys
                        .Select(r => r.GetShortDisplayName())
                        .ToList();
                    var record = new PbkModelDefinitionSummaryRecord() {
                        Code = definition.IdModelDefinition,
                        Name = definition.Name,
                        Description = definition.Description,
                        FileName = Path.GetFileName(definition.FileName),
                        TimeResolution = def.Resolution.GetDisplayName(),
                        ExposureRoutes = string.Join(", ", exposureRouteStrings),
                        OralInputCompartment = oral?.Compartment.Id,
                        DermalInputCompartment = dermal?.Compartment.Id,
                        InhalationInputCompartment = inhalation?.Compartment.Id
                    };
                    Records.Add(record);
                }
            }
        }
    }
}
