using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PbkModelDefinitionParameterSummaryRecord {

        [Description("Identifier of the parameter.")]
        [DisplayName("Parameter code")]
        public string ParameterCode { get; set; }

        [Description("Parameter name/description.")]
        [DisplayName("Description")]
        public string ParameterName { get; set; }

        [Description("The unit of measurement of the parameter.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("The interpreted type of the parameter.")]
        [DisplayName("Type")]
        public string Type { get; set; }
    }
}
