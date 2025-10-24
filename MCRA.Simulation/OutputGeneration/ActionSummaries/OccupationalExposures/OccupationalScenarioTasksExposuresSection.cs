using MCRA.General;
using MCRA.Simulation.Calculators.OccupationalScenarioExposureCalculation;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OccupationalScenarioTasksExposuresSection : ActionSummarySectionBase {

        public List<OccupationalScenarioTaskExposureRecord> Records { get; set; }

        public void Summarize(
            ICollection<OccupationalScenarioTaskExposure> occcupationalScenarioExposures
        ) {
            Records = occcupationalScenarioExposures
                .Select(t => {
                    return new OccupationalScenarioTaskExposureRecord() {
                        ScenarioCode = t.Scenario.Code,
                        ScenarioName = t.Scenario.Name,
                        TaskCode = t.Task.Code,
                        TaskName = t.Task.Name,
                        RpeType = t.Determinants.RPEType != RPEType.Undefined
                            ? t.Determinants.RPEType.GetShortDisplayName() : null,
                        ExposureRoute = t.Route.GetShortDisplayName(),
                        Unit = t.Unit.GetShortDisplayName(),
                        EstimateType = t.Unit.EstimateType != JobTaskExposureEstimateType.Undefined
                            ? t.Unit.EstimateType.GetDisplayName() : null,
                        SubstanceCode = t.Substance.Code,
                        SubstanceName = t.Substance.Name,
                        Duration = t.Duration,
                        Frequency = t.Frequency,
                        FrequencyResolution = t.FrequencyResolution.GetShortDisplayName(),
                        Exposure = t.Value
                    };
                })
                .OrderBy(c => c.ScenarioName)
                .ThenBy(c => c.TaskName)
                .ThenBy(c => c.ExposureRoute)
                .ThenBy(c => c.RpeType)
                .ToList();
        }
    }
}
