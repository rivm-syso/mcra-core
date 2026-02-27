using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class OccupationalTaskExposuresPerRouteRecord {

        [Description("Code of the scenari0.")]
        [DisplayName("Scenario code")]
        public string ScenarioCode { get; set; }

        [Description("Code of the task.")]
        [DisplayName("Task code")]
        public string TaskCode { get; set; }

        [Description("Name of the task.")]
        [DisplayName("Task name")]
        public string TaskName { get; set; }

        [Description("Code of the substance.")]
        [DisplayName("Substance code")]
        public string CodeSubstance { get; set; }

        [Description("Name of the substance.")]
        [DisplayName("Substance name")]
        public string NameSubstance { get; set; }

        [Description("Type respiratory protection equipment (RPE).")]
        [DisplayName("RPE type")]
        public string RpeType { get; set; }

        [Description("Type of hand protection.")]
        [DisplayName("Hand protection type")]
        public string HandProtectionType { get; set; }

        [Description("Type of protective clothing.")]
        [DisplayName("Protective clothing type")]
        public string ProtectiveClothingType { get; set; }

        [Description("Exposure route.")]
        [DisplayName("Route")]
        public string ExposureRoute { get; set; }
    }
}
