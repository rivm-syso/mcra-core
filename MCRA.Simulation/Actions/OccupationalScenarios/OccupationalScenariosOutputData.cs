using MCRA.Data.Compiled.Objects;
using MCRA.Simulation.Action;

namespace MCRA.Simulation.Actions.OccupationalScenarios {
    public class OccupationalScenariosOutputData : IModuleOutputData {
        public IDictionary<string, OccupationalScenario> OccupationalScenarios { get; set; }
        public IDictionary<string, OccupationalTask> OccupationalTasks { get; set; }
        public ICollection<OccupationalScenarioTask> OccupationalScenarioTasks { get; set; }
        public IModuleOutputData Copy() {
            return new OccupationalScenariosOutputData() {
                OccupationalScenarios = OccupationalScenarios,
                OccupationalTasks = OccupationalTasks,
                OccupationalScenarioTasks = OccupationalScenarioTasks,
            };
        }
    }
}
