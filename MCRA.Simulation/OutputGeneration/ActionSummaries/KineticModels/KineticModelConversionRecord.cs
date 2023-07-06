using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class KineticModelConversionRecord {

        [Description("Substance")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Biological matrix source")]
        [DisplayName("Biological matrix source")]
        public string Source { get; set; }

        [Description("Biological matrix target")]
        [DisplayName("Biological matrix target")]
        public string Target { get; set; }

        [Description("Conversion factor")]
        [DisplayName("Conversion factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConversionFactor { get; set; }
    }
}
