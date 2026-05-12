using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class OccupationalScenariosRecord {
        [Description("Code of the scenario.")]
        [DisplayName("Scenario code")]
        public string ScenarioCode { get; set; }

        [Description("Name of the scenario scenario.")]
        [DisplayName("Scenario name")]
        public string ScenarioName { get; set; }

        [Description("Description of the scenario.")]
        [DisplayName("Description")]
        public string Description { get; set; }

        [Description("Case control type ofthe scenario.")]
        [DisplayName("Case control")]
        public string CaseControl { get; set; }

        [Description("Number of tasks within the scenario.")]
        [DisplayName("Number of tasks")]
        public double NumTasks {  get; set; }

    }
}
