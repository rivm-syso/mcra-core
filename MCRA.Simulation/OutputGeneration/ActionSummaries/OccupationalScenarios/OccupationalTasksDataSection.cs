using MCRA.Data.Compiled.Objects;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class OccupationalTasksDataSection : SummarySection {
        public List<OccupationalTasksDataRecord> Records { get; set; }

        public void Summarize(
            IDictionary<string, OccupationalTask> OccupationalTasks
        ) {
            Records = [.. OccupationalTasks.Values
                .Select(c => {
                    return new OccupationalTasksDataRecord() {
                        TaskCode = c.Code,
                        TaskName = c.Name,
                        Description = c.Description
                    };
                })];
        }
    }
}