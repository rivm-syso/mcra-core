using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PbkModelParameterSummaryRecord {

        [Description("PBK model instance code.")]
        [DisplayName("Model instance code")]
        public string ModelInstanceCode { get; set; }

        [Description("PBK model instance name.")]
        [DisplayName("Model instance name")]
        public string ModelInstanceName { get; set; }

        [Description("Substance code.")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Substance name.")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Identifier of the parameter.")]
        [DisplayName("Parameter code")]
        public string ParameterCode { get; set; }

        [Description("Parameter name/description.")]
        [DisplayName("Description")]
        public string ParameterName { get; set; }

        [DisplayName("Value")]
        [DisplayFormat(DataFormatString = "{0:G5}")]
        public double Value { get; set; }

        [Description("Unit")]
        [DisplayName("Unit")]
        public string Unit { get; set; }
    }
}
