using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PbkModelDefinitionSummaryRecordRecord {

        [Description("The PBK model code.")]
        [DisplayName("Model code")]
        public string Code { get; set; }

        [Description("The name of the PBK model.")]
        [DisplayName("Model name")]
        public string Name { get; set; }

        [Description("Description of the PBK model.")]
        [DisplayName("Description")]
        public string Description { get; set; }

        [Description("File name.")]
        [DisplayName("File name")]
        public string FileName { get; set; }

        [Description("Input routes.")]
        [DisplayName("Input routes")]
        public string ExposureRoutes { get; set; }

        [Description("Oral input compartment.")]
        [DisplayName("Oral input compartment")]
        public string OralInpputCompartment { get; set; }

        [Description("Dermal input compartment.")]
        [DisplayName("Dermal input compartment")]
        public string DermalInpputCompartment { get; set; }

        [Description("Inhalation input compartment.")]
        [DisplayName("Inhalation input compartment")]
        public string InhalationInpputCompartment { get; set; }

    }
}
