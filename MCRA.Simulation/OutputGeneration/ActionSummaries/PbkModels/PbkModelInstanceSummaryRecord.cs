using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PbkModelInstanceSummaryRecord {

        [Description("The name of the PBK model instance/parametrisation.")]
        [DisplayName("Model instance name")]
        public string PbkModelInstanceName { get; set; }

        [Description("The code of the PBK model instance/parametrisation.")]
        [DisplayName("Model instance code")]
        public string PbkModelInstanceCode { get; set; }

        [Description("The name of the PBK model definition for which this instance is defined.")]
        [DisplayName("Model definition name")]
        public string PbkModelDefinitionName { get; set; }

        [Description("The code of the PBK model definition for which this instance is defined.")]
        [DisplayName("Model definition code")]
        public string PbkModelDefinitionCode { get; set; }

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
