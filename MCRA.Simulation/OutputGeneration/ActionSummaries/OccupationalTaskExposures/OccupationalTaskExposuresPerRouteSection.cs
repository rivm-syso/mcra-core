using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OccupationalTaskExposuresPerRouteSection : SummarySection {
        public List<OccupationalTaskExposuresPerRouteRecord> Records { get; set; } = [];

        public void Summarize(
            ICollection<OccupationalTaskExposure> occupationalTaskExposures,
            IDictionary<string, OccupationalScenario> occupationalScenarios,
            ICollection<Compound> substances
        ) {

            var lookups = occupationalScenarios.Values
                .SelectMany(c =>
                    c.Tasks,
                    (sc, c) => (
                        Scenario: sc,
                        OccupationalTask: c.OccupationalTask,
                        Determinants: c.Determinants()
                )).ToList();

            foreach (var item in lookups) {
                var results = occupationalTaskExposures
                    .Where(c => c.OccupationalTask == item.OccupationalTask
                        && c.Determinants() == item.Determinants)
                    .Select(c => (c.ExposureRoute, c.Substance));
                foreach (var result in results) {
                    Records.Add(new OccupationalTaskExposuresPerRouteRecord() {
                        CodeSubstance = result.Substance.Code,
                        NameSubstance = result.Substance.Name,
                        ExposureRoute = result.ExposureRoute.GetShortDisplayName(),
                        ScenarioCode = item.Scenario.Code,
                        TaskCode = item.OccupationalTask.Code,
                        TaskName = item.OccupationalTask.Name,
                        RpeType =  item.Determinants.RPEType != RPEType.Undefined ? item.Determinants.RPEType.GetDisplayName() : null,
                        HandProtectionType = item.Determinants.HandProtectionType != HandProtectionType.Undefined ? item.Determinants.HandProtectionType.GetDisplayName() : null,
                        ProtectiveClothingType = item.Determinants.ProtectiveClothingType != ProtectiveClothingType.Undefined ? item.Determinants.ProtectiveClothingType.GetDisplayName() : null,
                    });
                }
            }
        }
    }
}