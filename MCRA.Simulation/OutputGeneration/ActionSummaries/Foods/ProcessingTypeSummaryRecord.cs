using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ProcessingTypeSummaryRecord {

        [DisplayName("Processing type name")]
        public string Name { get; set; }

        [DisplayName("Processing type code")]
        public string Code { get; set; }

        [DisplayName("Bulking/Blending")]
        public string Bulking { get; set; }

        [DisplayName("Distribution")]
        public string Distribution { get; set; }
    }
}
