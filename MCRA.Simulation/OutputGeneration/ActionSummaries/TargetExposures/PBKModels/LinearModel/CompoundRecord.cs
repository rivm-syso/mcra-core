using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class CompoundRecord {
        [Description("Substance name")]
        [DisplayName("Name")]
        public string Name  { get; set; }

        [Description("Substance code")]
        [DisplayName("Code")]
        public string Code { get; set; }
    }
}
