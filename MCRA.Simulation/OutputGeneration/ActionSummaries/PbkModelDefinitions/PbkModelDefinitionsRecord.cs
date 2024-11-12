using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class PbkModelDefinitionsRecord {

        [Description("The name of the PBK model.")]
        [DisplayName("Model name")]
        public string Name { get; set; }

        [Description("Description of the PBK model.")]
        [DisplayName("Description")]
        public string Description { get; set; }

        [Description("File name.")]
        [DisplayName("File name")]
        public string FileName { get; set; }

    }
}
