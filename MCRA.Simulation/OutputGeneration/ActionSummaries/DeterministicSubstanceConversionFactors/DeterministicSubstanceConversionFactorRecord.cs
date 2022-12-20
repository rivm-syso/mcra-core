using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class DeterministicSubstanceConversionFactorRecord {

        [Description("Substance name of the measured substance.")]
        [DisplayName("Measured substance name")]
        public string MeasuredSubstanceName { get; set; }

        [Description("Substance code of the measured substance.")]
        [DisplayName("Measured substance code")]
        public string MeasuredSubstanceCode { get; set; }

        [Description("Substance name of the active substance.")]
        [DisplayName("Active substance name")]
        public string ActiveSubstanceName { get; set; }

        [Description("Substance code of the active substance.")]
        [DisplayName("Active substance code")]
        public string ActiveSubstanceCode { get; set; }

        [Description("Food name.")]
        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [Description("Food code.")]
        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Description("Specifies the conversion factor to translate concentrations of the measured substance to (equivalent)concentrations of the active substance according to e.g.the system used in PRIMo.")]
        [DisplayName("Conversion factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConversionFactor { get; set; }

        [Description("Reference to the source from which this value is obtained.")]
        [DisplayName("Reference")]
        public string Reference { get; set; }
    }
}
