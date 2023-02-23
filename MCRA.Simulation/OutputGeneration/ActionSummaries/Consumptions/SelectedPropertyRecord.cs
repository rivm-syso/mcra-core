using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.Consumptions {
    public sealed class SelectedPropertyRecord {
        [DisplayName("Property name")]
        public string PropertyName { get; set; }

        [DisplayName("Selected levels")]
        public string Levels { get; set; }
    }
}
