using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ParameterRecord {

        [Description("Substance code.")]
        [DisplayName("Substance code")]
        public string Code { get; set; }

        [Description("Substance name.")]
        [DisplayName("Substance name")]
        public string Name { get; set; }

        [Description("Parameter name.")]
        [DisplayName("Parameter name")]
        public string Parameter { get; set; }

        [DisplayName("Value")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Value { get; set; }

        [Description("Unit")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Description.")]
        [DisplayName("Description")]
        public string Description { get; set; }
    }
}
