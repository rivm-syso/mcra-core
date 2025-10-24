using MCRA.Data.Compiled.Objects;
using MCRA.General;
using MCRA.Utils.ExtensionMethods;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OccupationalScenarioTasksDataSection : SummarySection {
        public List<OccupationalScenarioTasksDataRecord> Records { get; set; }

        public void Summarize(
            IDictionary<string, OccupationalScenario> occupationalScenarios
        ) {
            Records = [.. occupationalScenarios.Values
                .SelectMany(c => c.Tasks
                .Select(r => {
                    return new OccupationalScenarioTasksDataRecord() {
                        ScenarioCode = c.Code,
                        NameScenario = c.Name,
                        TaskCode = r.OccupationalTask.Code,
                        TaskName = r.OccupationalTask.Name,
                        Duration = r.Duration,
                        Frequency = r.Frequency,
                        FrequencyResolution = r.FrequencyResolution.GetShortDisplayName(),
                        RpeType = r.RpeType != RPEType.Undefined
                            ? r.RpeType.ToString() : null
                    };
                })
            )];
        }
    }
}