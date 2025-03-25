using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {

    public sealed class KineticModelRecord {

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

        [Description("Dose unit.")]
        [DisplayName("Dose unit")]
        public string DoseUnit { get; set; }

        [Description("Modelled exposure routes.")]
        [DisplayName("Modelled exposure routes")]
        public string Routes { get; set; }

        [Description("Output compartment.")]
        [DisplayName("Output compartment")]
        public string Output { get; set; }

        [Description("Output unit.")]
        [DisplayName("Output unit")]
        public string OutputUnit { get; set; }

        [Description("Time unit.")]
        [DisplayName("Time unit")]
        public string TimeUnit { get; set; }

    }
}
