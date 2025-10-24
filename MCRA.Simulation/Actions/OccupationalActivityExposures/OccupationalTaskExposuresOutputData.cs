using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.OccupationalTaskExposures {
    public class OccupationalTaskExposuresOutputData : IModuleOutputData {
        public IDictionary<string, OccupationalScenario> OccupationalScenarios { get; set; }
        public IDictionary<string, OccupationalTask> OccupationalTasks { get; set; }
        public ICollection<OccupationalScenarioTask> OccupationalScenarioTasks { get; set; }
        public ICollection<OccupationalTaskExposure> OccupationalTaskExposures { get; set; }

        public IModuleOutputData Copy() {
            return new OccupationalTaskExposuresOutputData() {
                OccupationalScenarios = OccupationalScenarios,
                OccupationalTasks = OccupationalTasks,
                OccupationalScenarioTasks = OccupationalScenarioTasks,
                OccupationalTaskExposures = OccupationalTaskExposures
            };
        }
    }
}
