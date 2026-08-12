using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductExposureSetRecord {
        [Description("Assessment name.")]
        [DisplayName("Assessment")]
        public string Name { get; set; }

        [Description("Assessment code.")]
        [DisplayName("Assessment code")]
        public string Code { get; set; }

        [Description("Exposure set code.")]
        [DisplayName("Exposure set code")]
        public string ExposureSetCode { get; set; }

        [Description("Substance name.")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code.")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Consumer products.")]
        [DisplayName("Consumer products")]
        public string Products { get; set; }

        [Description("Exposure routes.")]
        [DisplayName("Exposure routes")]
        public string Routes { get; set; }

        [Description("Number of percentiles.")]
        [DisplayName("Number of percentiles")]
        [DisplayFormat(DataFormatString = "{0:N0}")]
        public int NumberOfPercentiles { get; set; }

        [Description("Minimum percentage and value, unless all values are zero.")]
        [DisplayName("Minimum")]
        public string Minimum { get; set; }

        [Description("Maximum percentage and value, unless all values are zero.")]
        [DisplayName("Maximum")]
        public string Maximum { get; set; }

        [Description("Percentage of zeros.")]
        [DisplayName("Zeros (%)")]
        [DisplayFormat(DataFormatString = "{0:0}")]
        public double PercentageOfZeros { get; set; }

        [Description("Unit of measurement.")]
        [DisplayName("Unit")]
        public string Unit { get; set; }
    }
}
