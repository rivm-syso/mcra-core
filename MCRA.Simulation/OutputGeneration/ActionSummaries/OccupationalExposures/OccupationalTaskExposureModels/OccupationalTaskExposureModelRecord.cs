using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class OccupationalTaskExposureModelRecord {

        [Description("Code of the task.")]
        [DisplayName("Task code")]
        public string TaskCode { get; set; }

        [Description("Name of the task.")]
        [DisplayName("Task name")]
        public string TaskName { get; set; }

        [Description("Type of respiratory protection equipment (RPE).")]
        [DisplayName("RPE Type")]
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

        [Description("Identification code of the substance.")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Name of the substance.")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Unit.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Type of the model.")]
        [DisplayName("Fitted model type")]
        public string ModelType { get; set; }

        [Description("Value.")]
        [DisplayName("Value")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Value { get; set; }

        [Description("Model basis.")]
        [DisplayName("Model basis")]
        public string ModelBasis { get; set; }

    }
}
