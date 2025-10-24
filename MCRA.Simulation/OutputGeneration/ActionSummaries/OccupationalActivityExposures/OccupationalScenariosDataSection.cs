using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OccupationalScenariosDataSection : SummarySection {
        public List<OccupationalScenariosRecord> Records { get; set; }

        public void Summarize(
            IDictionary<string, OccupationalScenario> occupationalScenarios
        ) {
            Records = [.. occupationalScenarios.Values
                .Select(c => {
                    return new OccupationalScenariosRecord() {
                        ScenarioCode = c.Code,
                        ScenarioName = c.Name,
                        Description = c.Description,
                        NumTasks = c.Tasks.Count
                    };
                })];
        }
    }
}