using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class ConsumerProductsSummaryRecord {

        [DisplayName("Consumer product name")]
        public string Name { get; set; }

        [DisplayName("Consumer product code")]
        public string Code { get; set; }

        [DisplayName("Group code")]
        public string CodeParent { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }
    }
}
