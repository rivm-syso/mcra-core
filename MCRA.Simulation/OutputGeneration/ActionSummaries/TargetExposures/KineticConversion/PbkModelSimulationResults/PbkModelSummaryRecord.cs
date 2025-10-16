using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class PbkModelSummaryRecord {

        [Description("Model name.")]
        [DisplayName("Model name")]
        public string ModelName { get; set; }

        [Description("Model code.")]
        [DisplayName("Model code")]
        public string ModelCode { get; set; }

        [Description("Model instance code.")]
        [DisplayName("Model instance code")]
        public string ModelInstanceCode { get; set; }

        [Description("Substance name.")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code.")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Modelled exposure routes.")]
        [DisplayName("Modelled exposure routes")]
        public string Routes { get; set; }

        [Description("Exposure target.")]
        [DisplayName("Exposure target")]
        public string ExposureTarget { get; set; }

    }
}
