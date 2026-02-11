using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class OccupationalTaskExposuresDataRecord {

        [Description("Code of the task.")]
        [DisplayName("Task code")]
        public string TaskCode { get; set; }

        [Description("Name of the task.")]
        [DisplayName("Task name")]
        public string TaskName { get; set; }

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
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public string ExposureRoute { get; set; }

        [Description("Unit.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Estimate type.")]
        [DisplayName("Estimate type")]
        public string EstimateType { get; set; }

        [Description("Code of the substance.")]
        [DisplayName("Substance code")]
        public string CodeSubstance { get; set; }

        [Description("Name of the substance.")]
        [DisplayName("Substance name")]
        public string NameSubstance { get; set; }

        [Description("Percentile point from the distribution.")]
        [DisplayName("Percentage")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Percentage { get; set; }

        [Description("Exposure estimate value.")]
        [DisplayName("Value")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Value { get; set; }

        [Description("Reference/source from which the exposure estimate was obtained.")]
        [DisplayName("Reference")]
        public string Reference { get; set; }
    }
}
