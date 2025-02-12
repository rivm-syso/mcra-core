using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration.ActionSummaries.HumanMonitoringData {
    public sealed class HbmConcentrationsPercentilesRecord : PercentilesRecordBase {

        [Description("Substance name")]
        [DisplayName("Substance name")]
        public string SubstanceName { get; set; }

        [Description("Substance code")]
        [DisplayName("Substance code")]
        public string SubstanceCode { get; set; }

        [Description("Description, e.g. analytical method, sampling type.")]
        [DisplayName("Description")]
        public string Description { get; set; }
    }
}
