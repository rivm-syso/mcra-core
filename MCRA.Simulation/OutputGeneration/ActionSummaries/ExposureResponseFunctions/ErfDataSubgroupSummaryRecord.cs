using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
namespace MCRA.Simulation.OutputGeneration {
    public class ErfDataSubgroupSummaryRecord {

        [Description("The code of the exposure response function.")]
        [DisplayName("ERF Code")]
        public string ExposureResponseFunctionCode { get; set; }

        [Description("The code of the subgroup.")]
        [DisplayName("Subgroup code")]
        public string ErSubgroupCode { get; set; }

        [Description("The upper threshold of the exposure in the subgroup.")]
        [DisplayName("Exposure threshold upper")]
        [DisplayFormat(DataFormatString = "{0:G3}")]
        public double? ExposureUpper { get; set; }

        [Description("The exposure response function specification.")]
        [DisplayName("Exposure response specification")]
        public string ExposureResponseSpecification { get; set; }

        [Description("The uncertainty distribution type of the exposure response specification.")]
        [DisplayName("ER specification uncertainty distribution")]
        public string ErSpecificationUncertaintyType { get; set; }

        [Description("The lower threshold of the exposure response specification.")]
        [DisplayName("ER uncertainty lower")]
        public string ErSpecificationUncLower { get; set; }

        [Description("The upper threshold of the exposure response specification.")]
        [DisplayName("ER uncertainty upper")]
        public string ErSpecificationUncUpper { get; set; }
    }
}
