using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PbkModelDefinitionsOverviewSection : SummarySection {

        public List<PbkModelDefinitionSummaryRecord> Records { get; set; } = [];

        public void Summarize(ICollection<PbkModelDefinition> pbkModelDefinitions) {
            foreach (var definition in pbkModelDefinitions) {
                var def = definition.KineticModelDefinition;
                if (def.Format == PbkModelType.SBML) {
                    // We only include SBML PBK models in this table and skip the hard-coded DeSolve models
                    var inputs = def.GetInputDefinitions();
                    var oral = inputs
                        .Where(r => r.Route == ExposureRoute.Oral)
                        .Select(r => r.Id)
                        .FirstOrDefault();
                    var dermal = inputs
                        .Where(r => r.Route == ExposureRoute.Dermal)
                        .Select(r => r.Id)
                        .FirstOrDefault();
                    var inhalation = inputs
                        .Where(r => r.Route == ExposureRoute.Inhalation)
                        .Select(r => r.Id)
                        .FirstOrDefault();
                    var exposureRouteStrings = def.GetExposureRoutes()
                        .Select(r => r.GetShortDisplayName())
                        .ToList();
                    var record = new PbkModelDefinitionSummaryRecord() {
                        Code = definition.IdModelDefinition,
                        Name = definition.Name,
                        Description = definition.Description,
                        FileName = Path.GetFileName(definition.FileName),
                        TimeResolution = def.Resolution.GetDisplayName(),
                        ExposureRoutes = string.Join(", ", exposureRouteStrings),
                        OralInputCompartment = oral,
                        DermalInputCompartment = dermal,
                        InhalationInputCompartment = inhalation
                    };
                    Records.Add(record);
                }
            }
        }
    }
}
