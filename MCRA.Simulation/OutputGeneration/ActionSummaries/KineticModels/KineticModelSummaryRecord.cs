using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class KineticModelSummaryRecord {

        [Description("The kinetic model instance/parametrisation.")]
        [DisplayName("Model instance name")]
        public string ModelInstanceName { get; set; }

        [Description("The kinetic model instance/parametrisation.")]
        [DisplayName("Model instance code")]
        public string ModelInstanceCode { get; set; }

        [Description("The name of the kinetic model used for the specified substance.")]
        [DisplayName("Model name")]
        public string KineticModelName { get; set; }

        [Description("The code of the kinetic model used for the specified substance.")]
        [DisplayName("Model code")]
        public string KineticModelCode { get; set; }

        [Description("Name(s) of the substance(s) for which this model instance is defined.")]
        [DisplayName("Substance name(s)")]
        public string SubstanceNames { get; set; }

        [Description("Code(s) of the substance(s) for which this model instance is defined.")]
        [DisplayName("Substance code")]
        public string SubstanceCodes { get; set; }

        [Description("The species to which this model applies.")]
        [DisplayName("Species")]
        public string Species { get; set; }

        [Description("Name(s) of the substance(s) available as inputs of the model.")]
        [DisplayName("Input substance name(s)")]
        public string InputSubstanceNames { get; set; }

        [Description("Code(s) of the substance(s) available as inputs of the model.")]
        [DisplayName("Input substance code(s)")]
        public string InputSubstanceCodes { get; set; }
    }
}
