using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public class PopulationCharacteristicsSummaryRecord {

        [Description("Population code.")]
        [DisplayName("Population code")]
        public string PopulationCode { get; set; }

        [Description("Population characteristic.")]
        [DisplayName("Characteristic")]
        public string Characteristic { get; set; }

        [Description("Unit of the characteristic.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }

        [Description("Distribution type.")]
        [DisplayName("Distribution type")]
        public string DistributionType { get; set; }

        [Description("Value.")]
        [DisplayName("Value")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Value { get; set; }

        [Description("Coefficient of variation.")]
        [DisplayName("CV")]
        [DisplayFormat(DataFormatString = "{0:G5}")]
        public double? CV { get; set; }
    }
}
