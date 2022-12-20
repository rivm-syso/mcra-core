using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class SubstanceConversionsDataRecord {

        [DisplayName("Measured substance name")]
        public string MeasuredSubstanceName { get; set; }

        [DisplayName("Measured substance code")]
        public string MeasuredSubstanceCode { get; set; }

        [DisplayName("Active substance name")]
        public string ActiveSubstanceName { get; set; }

        [DisplayName("Active substance code")]
        public string ActiveSubstanceCode { get; set; }

        [DisplayName("Conversion factor")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double ConversionFactor { get; set; }

        [DisplayName("Exclusive (y/n)")]
        public bool IsExclusive { get; set; }

        [DisplayName("Proportion")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? Proportion { get; set; }

    }
}
