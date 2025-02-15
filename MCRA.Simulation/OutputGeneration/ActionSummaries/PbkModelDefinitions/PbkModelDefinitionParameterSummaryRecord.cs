using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.General;

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

        [Description("Default value of the parameter.")]
        [DisplayName("Default value")]
        [DisplayFormat(DataFormatString = "{0:G5}")]
        public double? Value { get; set; }

        [Description("The interpreted type of the parameter.")]
        [DisplayName("Type")]
        public string ParameterType {
            get {
                return Type != PbkModelParameterType.Undefined
                    ? Type.ToString()
                    : null;
            }
        }

        [Display(AutoGenerateField = false)]
        public PbkModelParameterType Type { get; set; }
    }
}
