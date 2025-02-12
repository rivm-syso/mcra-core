using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.DustExposures {
    public sealed class DustExposuresPercentilesRecord : PercentilesRecordBase{

        [Description("Exposure route")]
        [DisplayName("Exposure route")]
        public string ExposureRoute { get; set; }

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Description, e.g. analytical method, sampling type.")]
        [DisplayName("Description")]
        public string Description { get; set; }
    }
}
