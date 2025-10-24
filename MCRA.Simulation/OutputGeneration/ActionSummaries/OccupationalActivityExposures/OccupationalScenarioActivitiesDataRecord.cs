using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class OccupationalScenarioTasksDataRecord {
        [Description("Code of the scenario.")]
        [DisplayName("Scenario code")]
        public string ScenarioCode { get; set; }

        [Description("Name of the scenario.")]
        [DisplayName("Scenario name")]
        public string NameScenario { get; set; }

        [Description("Code of the task.")]
        [DisplayName("Task code")]
        public string TaskCode { get; set; }

        [Description("Name of the task.")]
        [DisplayName("Task name")]
        public string TaskName { get; set; }

        [Description("Duration.")]
        [DisplayName("Duration")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Duration { get; set; }

        [Description("Frequency.")]
        [DisplayName("Frequency")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Frequency { get; set; }

        [Description("Frequency resolution.")]
        [DisplayName("Frequency resolution")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public string FrequencyResolution { get; set; }

        [Description("RPE Type.")]
        [DisplayName("RPE Type")]
        public string RpeType { get; set; }
    }
}
