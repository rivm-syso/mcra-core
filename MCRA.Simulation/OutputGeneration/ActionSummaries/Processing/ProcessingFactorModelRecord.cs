using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ProcessingFactorModelRecord {

        [Description("Name of the raw food.")]
        [DisplayName("Food name")]
        public string FoodName { get; set; }

        [Description("Code of the raw food.")]
        [DisplayName("Food code")]
        public string FoodCode { get; set; }

        [Description("Name of the substance for which this processing factor applies.")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Code of the substance for which this processing factor applies.")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("The processing type of this processing factor.")]
        [DisplayName("Processing type")]
        public string ProcessingTypeName { get; set; }

        [Description("The processing type of this processing factor.")]
        [DisplayName("Processing type code")]
        public string ProcessingTypeCode { get; set; }

        [Description("The nominal (best estimate of 50th percentile) processing factor.")]
        [DisplayName("Nominal")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Nominal { get; set; }

        [Description("Specifies whether the processing is applied on large batches.")]
        [DisplayName("Bulking/blending")]
        public string BulkingBlending { get; set; }

        [Description("The distribution type of the processing factor.")]
        [DisplayName("Distribution")]
        public string Distribution { get; set; }

        [Description("Parameter mu of the processing factor distribution.")]
        [DisplayName("Mu")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Mu { get; set; }

        [Description("Parameter sigma of the processing factor distribution.")]
        [DisplayName("Sigma")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double Sigma { get; set; }

    }
}
