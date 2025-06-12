using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class ConsumerProductApplicationAmountRecord {
        [DisplayName("Product code")]
        public string ProductCode { get; set; }

        [DisplayName("Product name")]
        public string ProductName { get; set; }

        [Description("Either the mean value of the distribution if a distribution is specified. Otherwise the constant value of the parameter")]
        [DisplayName("Mean")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Amount { get; set; }

        [Description("The distribution type.")]
        [DisplayName("Distribution")]
        public string DistributionType { get; set; }

        [Description("The coefficient of variation of the distribution.")]
        [DisplayName("CV")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? CvVariability { get; set; }
    }
}

