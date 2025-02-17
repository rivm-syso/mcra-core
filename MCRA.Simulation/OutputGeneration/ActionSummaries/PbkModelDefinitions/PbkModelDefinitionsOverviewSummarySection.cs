using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PbkModelDefinitionsOverviewSummarySection : SummarySection {

        public List<PbkModelDefinitionSummaryRecordRecord> Records { get; set; } = [];

        public void Summarize(ICollection<PbkModelDefinition> pbkModelDefinitions) {
            foreach (var definition in pbkModelDefinitions) {
                var def = definition.KineticModelDefinition;
                if (def.Format == KineticModelType.SBML) {
                    // We only include SBML PBK models in this table and skip the hard-coded DeSolve models
                    var oral = def.Forcings
                        .Where(r => r.Route == ExposureRoute.Oral)
                        .Select(r => r.Id)
                        .FirstOrDefault();
                    var dermal = def.Forcings
                        .Where(r => r.Route == ExposureRoute.Dermal)
                        .Select(r => r.Id)
                        .FirstOrDefault();
                    var inhalation = def.Forcings
                        .Where(r => r.Route == ExposureRoute.Inhalation)
                        .Select(r => r.Id)
                        .FirstOrDefault();
                    var exposureRouteStrings = def.GetExposureRoutes()
                        .Select(r => r.GetShortDisplayName())
                        .ToList();
                    var record = new PbkModelDefinitionSummaryRecordRecord() {
                        Code = definition.IdModelDefinition,
                        Name = definition.Name,
                        Description = definition.Description,
                        FileName = Path.GetFileName(definition.FileName),
                        ExposureRoutes = string.Join(", ", exposureRouteStrings),
                        OralInpputCompartment = oral,
                        DermalInpputCompartment = dermal,
                        InhalationInpputCompartment = inhalation
                    };
                    Records.Add(record);
                }
            }
        }
    }
}
