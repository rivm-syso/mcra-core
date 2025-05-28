using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MCRA.Data.Compiled.Objects;
using MCRA.General;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ConsumerProductExposureFractionRecord {
        [DisplayName("Product code")]
        public string ProductCode { get; set; }

        [DisplayName("Product name")]
        public string ProductName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        public string Route { get; set; }

        [Description("The exposure fraction.")]
        [DisplayName("Fraction")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Fraction { get; set; }
    }
}

