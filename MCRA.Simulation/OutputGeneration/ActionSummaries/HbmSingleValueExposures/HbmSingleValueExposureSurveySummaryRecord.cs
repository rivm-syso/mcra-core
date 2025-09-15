using System.ComponentModel;

namespace MCRA.Simulation.OutputGeneration {
    public sealed class HbmSingleValueExposureSurveySummaryRecord {

        [DisplayName("Survey name")]
        public string Name { get; set; }

        [DisplayName("Country")]
        public string Country { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }
    }
}
