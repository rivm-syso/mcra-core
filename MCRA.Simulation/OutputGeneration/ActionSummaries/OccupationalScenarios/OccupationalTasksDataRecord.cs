using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class OccupationalTasksDataRecord {

        [Description("Code of the task.")]
        [DisplayName("Task code")]
        public string TaskCode { get; set; }

        [Description("Name of the task.")]
        [DisplayName("Task name")]
        public string TaskName { get; set; }

        [Description("Description.")]
        [DisplayName("Description")]
        public string Description { get; set; }
    }
}
