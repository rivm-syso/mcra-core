using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PbkModelDefinitionStateVariablesSummaryRecord {

        [Description("Identifier of the output compartment.")]
        [DisplayName("Compartment code")]
        public string CompartmentCode { get; set; }

        [Description("Name/description of the output compartment.")]
        [DisplayName("Compartment name")]
        public string CompartmentName { get; set; }

        [Description("The biological matrix associated with the compartment of the output.")]
        [DisplayName("Biological matrix")]
        public string BiologicalMatrix { get; set; }
    }
}
