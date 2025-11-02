using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PbkModelDefinitionSpeciesSummaryRecord {

        [Description("Identifier of the compartment.")]
        [DisplayName("Compartment code")]
        public string CompartmentCode { get; set; }

        [Description("Name/description of the compartment.")]
        [DisplayName("Compartment name")]
        public string CompartmentName { get; set; }

        [Description("Identifier of the (chemical) species.")]
        [DisplayName("Species code")]
        public string SpeciesCode { get; set; }

        [Description("Name/description of the (chemical) species.")]
        [DisplayName("Species name")]
        public string SpeciesName { get; set; }

        [Description("The biological matrix associated with the compartment of the output.")]
        [DisplayName("Biological matrix")]
        public string BiologicalMatrix { get; set; }

        [Description("Amount unit of the species.")]
        [DisplayName("Species amount unit")]
        public string SpeciesAmountUnit { get; set; }

        [Description("Volume unit of the compartment.")]
        [DisplayName("Compartment volume unit")]
        public string CompartmentVolumeUnit { get; set; }
    }
}
