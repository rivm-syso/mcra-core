using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class OccupationalScenarioTaskExposureRecord {

        [Description("Scenario code.")]
        [DisplayName("Scenario code")]
        public string ScenarioCode { get; set; }

        [Description("Name of the exposure scenario.")]
        [DisplayName("Scenario name")]
        public string ScenarioName { get; set; }

        [Description("Code of the occupational task.")]
        [DisplayName("Code task")]
        public string TaskCode { get; set; }

        [Description("Name of the occupational task.")]
        [DisplayName("Name task")]
        public string TaskName { get; set; }

        [Description("Type of repiratory protection equipment.")]
        [DisplayName("RPE Type")]
        public string RpeType { get; set; }

        [Description("Substance code.")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Substance name.")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Exposure route.")]
        [DisplayName("Route")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public string ExposureRoute { get; set; }

        [Description("Duration of the task within the scenario (in minutes).")]
        [DisplayName("Task duration (min)")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Duration { get; set; }

        [Description("Frequency with which the task is executed within the scenario.")]
        [DisplayName("Frequency")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Frequency { get; set; }

        [Description("Resolution of the task frequency.")]
        [DisplayName("Frequency resolution")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public string FrequencyResolution { get; set; }

        [Description("Exposure estimate value.")]
        [DisplayName("Exposure")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Exposure { get; set; }

        [Description("Exposure estimate unit.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Estimate type.")]
        [DisplayName("Estimate type")]
        public string EstimateType { get; set; }

    }
}
