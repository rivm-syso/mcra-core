using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class KineticModelSubstanceRecord {

        [Description("Identifier of the kinetic model instance/parametrization.")]
        [DisplayName("Kinetic model instance code")]
        public string KineticModelInstanceCode { get; set; }

        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("PBK substance code.")]
        [DisplayName("PBK substance code")]
        public string KineticModelSubstanceCode { get; set; }

        [Description("PBK substance name.")]
        [DisplayName("PBK substance name")]
        public string KineticModelSubstanceName { get; set; }

    }
}
